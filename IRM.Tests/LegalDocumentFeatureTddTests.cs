using IRM.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace IRM.Tests;

public sealed class LegalDocumentFeatureTddTests
{
    [Fact]
    public void ValidateUploadMetadata_ChecksExtensionsAndMime()
    {
        // Whitelisted extension and matching MIME
        LegalDocumentService.ValidateUploadMetadata(".pdf", "application/pdf");
        LegalDocumentService.ValidateUploadMetadata(".png", "image/png");
        LegalDocumentService.ValidateUploadMetadata(".jpg", "image/jpeg");
        LegalDocumentService.ValidateUploadMetadata(".jpeg", "image/jpeg");

        // Disallowed extension
        var exExt = Assert.Throws<InvalidOperationException>(() =>
            LegalDocumentService.ValidateUploadMetadata(".exe", "application/octet-stream"));
        Assert.Contains("Định dạng", exExt.Message);

        // Mime mismatch
        var exMime = Assert.Throws<InvalidOperationException>(() =>
            LegalDocumentService.ValidateUploadMetadata(".pdf", "image/png"));
        Assert.Contains("Định dạng", exMime.Message);
    }

    [Fact]
    public void HasAllowedSignature_ValidatesMagicBytes()
    {
        // PDF magic bytes: %PDF (0x25, 0x50, 0x44, 0x46)
        var validPdf = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
        Assert.True(LegalDocumentService.HasAllowedSignature(validPdf, ".pdf"));

        // Fake PDF with MZ executable bytes (0x4D, 0x5A)
        var fakePdf = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
        Assert.False(LegalDocumentService.HasAllowedSignature(fakePdf, ".pdf"));

        // PNG magic bytes: 89 50 4E 47 0D 0A 1A 0A
        var validPng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        Assert.True(LegalDocumentService.HasAllowedSignature(validPng, ".png"));

        // JPEG magic bytes: FF D8 FF
        var validJpg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
        Assert.True(LegalDocumentService.HasAllowedSignature(validJpg, ".jpg"));
        Assert.True(LegalDocumentService.HasAllowedSignature(validJpg, ".jpeg"));
    }

    [Fact]
    public async Task StoreAsync_DeletesTemporaryFile_OnMalwareDetection()
    {
        await using var db = new TestDatabase();
        await db.InitializeAsync();

        var root = Path.Combine(Path.GetTempPath(), "irm-legal-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        try
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["FileStorage:Root"] = root
            }).Build();

            var env = new TestHostingEnvironment(root);
            var service = new LegalDocumentService(
                db.Context,
                new AuditService(db.Context),
                config,
                env,
                TestSecurityContext.CreateAllowAllGuard(),
                new RejectingMalwareScanner()
            );

            var validPdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
            using var stream = new MemoryStream(validPdfBytes);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.StoreAsync(stream, "license.pdf", "application/pdf"));

            // Verification: File deleted on disk, no DB row inserted
            Assert.Empty(Directory.GetFiles(root));
            Assert.Empty(db.Context.StoredFiles);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private sealed class RejectingMalwareScanner : IMalwareScanner
    {
        public Task ScanAsync(string path, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Phát hiện mã độc trong tệp tin!");
    }

    private sealed class TestHostingEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IRM.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
