using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportManager.Bases
{
    public static class Constants
    {
        // connect to sql server
        public static string Source = "DESKTOP-BJ63NDN\\BIRDIEPO";
        public static string Catalog = "ReportManagerDB";
        public static string IDSQLServer = "sa";
        public static string PasswordSQLServer = "123456";
        public static string PathShare = @"\\172.17.103.226\data_share";
        public static string AccountShare = "admin";
        public static string PassShare = "ifv848484";
        //open code uc
        public const int OpenListFieldUC = 1;
        public const int OpenListCareersUC = 2;
        public const int OpenListAccountsUC = 3;
        public const int OpenListNationalitiesUC = 4;
        public const int OpenListDistrictsUC = 5;
        public const int OpenListWardsUC = 6;
        // info account
        public static string IdUser = "";
        public static string Username = "";
        public static string Password = "";
        public static string Permission = "";
        // info work permit
        public const int WorkPermitExemption = 0;
        public const int WorkPermitAvailable = 1;
        public const int WorkPermitNotYet = 2;
        public const int WorkPermitExemption_Invest = 3;
        public const int WorkPermitAvailable_Invest = 4;
        public const int WorkPermitNotYet_Invest = 5;
        public const int WorkPermitOther = 6;

        // info career other
        public static string OtherCareerGroup = "";
    }
}
