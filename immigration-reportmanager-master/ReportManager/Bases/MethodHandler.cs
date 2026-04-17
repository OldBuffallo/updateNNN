using ReportManager.Models;
using ReportManager.UserControls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ReportManager.Bases
{
    class MethodHandler
    {
        public static ObservableCollection<string> getDistricts()
        {
            ObservableCollection<string> listDistricts = new ObservableCollection<string>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Districts where Delete_flag = 0 order by DisTrictName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listDistricts.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string rowDistrictName = dr["DisTrictName"].ToString();
                            listDistricts.Add(rowDistrictName);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return listDistricts;
            }
        }
        public static ObservableCollection<string> getWards()
        {
            ObservableCollection<string> listWards = new ObservableCollection<string>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Wards where Delete_flag = 0 order by WardName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listWards.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string rowDistrictName = dr["WardName"].ToString();
                            listWards.Add(rowDistrictName);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return listWards;
            }
        }
        public static ObservableCollection<string> getPassportByCompany(int idCompany)
        {
            ObservableCollection<string> listPassport = new ObservableCollection<string>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                DateTime today = DateTime.Today;
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select Passport from Employees, Nationality where Employees.Nationality like Nationality.NationalityCode and IDCompany = "
                        + idCompany + " and Hidden_flag = 0"
                        + " and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')"
                        + " and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "'";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listPassport.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            string rowPassport = dr["Passport"].ToString();
                            listPassport.Add(rowPassport);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return listPassport;
            }
        }
        public static Employee getEmployeeByPassport(int idCompany, string passport)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                DateTime today = DateTime.Today;
                Employee rowEmployee = new Employee();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select CONVERT(varchar, Birthday, 105) birth, CONVERT(varchar, TemporaryStay, 105) temporary,"
                        + " CONVERT(varchar, DateOfJoin, 105) datejoin, CONVERT(varchar, DateOfLeave, 105) dateleave, Employees.Hidden_flag hidden, *"
                        + " from Employees, Nationality where Employees.Nationality like Nationality.NationalityCode and IDCompany = "
                        + idCompany + " and Hidden_flag = 0 and Employees.Passport like N'" + passport + "'"
                        + " and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')"
                        + " and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' order by IDCareer, Address, StaffName";
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            rowEmployee.IDEmployee = int.Parse(dr["IDEmployee"].ToString());
                            rowEmployee.StaffName = dr["StaffName"].ToString().Trim();
                            rowEmployee.Gender = int.Parse(dr["Gender"].ToString());
                            rowEmployee.Birthday = dr["birth"].ToString().Trim();
                            rowEmployee.Nationality.NationalityCode = dr["NationalityCode"].ToString().Trim();
                            rowEmployee.Nationality.NationalityName = dr["NationalityName"].ToString().Trim();
                            rowEmployee.Passport = dr["Passport"].ToString().Trim();
                            rowEmployee.Address = dr["Address"].ToString().Trim();
                            rowEmployee.Career.IDCareer = int.Parse(dr["IDCareer"].ToString());
                            rowEmployee.WorkPermit = int.Parse(dr["WorkPermit"].ToString());
                            rowEmployee.WorkPermitNumber = dr["WorkPermitNumber"].ToString().Trim();
                            rowEmployee.VisaNumber = dr["VisaNumber"].ToString().Trim();
                            rowEmployee.TemporaryStay = dr["temporary"].ToString().Trim();
                            rowEmployee.SettlementResults = int.Parse(dr["SettlementResults"].ToString());
                            rowEmployee.SettlementResultsString = dr["SettlementResultsString"].ToString().Trim();
                            rowEmployee.IDUser = int.Parse(dr["IDUser"].ToString());
                            rowEmployee.IDCompany = int.Parse(dr["IDCompany"].ToString());
                            rowEmployee.Note = dr["Note"].ToString().Trim();
                            rowEmployee.WorkingStatus = int.Parse(dr["WorkingStatus"].ToString());
                            rowEmployee.DateOfJoin = dr["datejoin"].ToString().Trim();
                            rowEmployee.DateOfLeave = dr["dateleave"].ToString().Trim();
                            rowEmployee.Hidden = int.Parse(dr["hidden"].ToString());
                            // get career
                            DataTable dtGet = new DataTable();
                            adapter.SelectCommand = new SqlCommand("select CareerName, IDCG from Careers where IDCareer = " + rowEmployee.Career.IDCareer, con);
                            adapter.Fill(dtGet);
                            if (dtGet.Rows.Count > 0)
                            {
                                foreach (DataRow row in dtGet.Rows)
                                {
                                    rowEmployee.Career.CareerName = row["CareerName"].ToString().Trim();
                                    rowEmployee.Career.IDCG = int.Parse(row["IDCG"].ToString());
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return rowEmployee;
            }
        }
        public static ObservableCollection<Nationality> getNationalities()
        {
            ObservableCollection<Nationality> listNationalities = new ObservableCollection<Nationality>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from Nationality where Delete_flag = 0 order by NationalityCode, NationalityName", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listNationalities.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            Nationality rowNationality = new Nationality();
                            rowNationality.IDNationality = int.Parse(dr["IDNationality"].ToString());
                            rowNationality.NationalityCode = dr["NationalityCode"].ToString();
                            rowNationality.NationalityName = dr["NationalityName"].ToString();
                            listNationalities.Add(rowNationality);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return listNationalities;
            }
        }
        public static int getCareerID(string IDCG, string nameCareer)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    string sql = "select * from Careers where IDCG = " + IDCG + " and CareerName like N'" + nameCareer + "' and Delete_flag = 0";
                    adapter.SelectCommand = new SqlCommand(sql , con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in dt.Rows)
                        {
                            return int.Parse(dr["IDCareer"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                finally
                {
                    con.Close();
                }
                return -1;
            }
        }
        public static string formatDatefromUsertoDatabase(string strDate)
        {
            // check strDate before split string
            if (string.IsNullOrWhiteSpace(strDate))
            {
                return null;
            }
            int check1 = strDate.IndexOf('-');
            int check2 = strDate.IndexOf('/');
            if (check1 >= 0)
            {
                // convert from dd-mm-yyyy to yyyy-mm-dd
                string[] dmy = strDate.Split('-');
                if (dmy.Length == 3)
                {
                    // format type dd/MM/yy
                    dmy[2] = ((int.Parse(dmy[2]) < 40) && (int.Parse(dmy[2]) >= 0)) ? "" + (2000 + int.Parse(dmy[2])) : ((int.Parse(dmy[2]) < 100) && (int.Parse(dmy[2]) >= 40)) ? "" + (1900 + int.Parse(dmy[2])) : dmy[2];
                    dmy[1] = int.Parse(dmy[1]) < 10 ? "0" + int.Parse(dmy[1]) : dmy[1];
                    dmy[0] = int.Parse(dmy[0]) < 10 ? "0" + int.Parse(dmy[0]) : dmy[0];
                    return dmy[2] + "-" + dmy[1] + "-" + dmy[0];
                }
                else
                {
                    MessageBox.Show("Sai định dạng ngày tháng");
                    return null;
                }
            } else if(check2 >= 0)
            {
                // convert from dd/mm/yyyy to yyyy-mm-dd
                string[] dmy = strDate.Split('/');
                if (dmy.Length == 3)
                {
                    // format type dd/MM/yy
                    dmy[2] = ((int.Parse(dmy[2]) < 40) && (int.Parse(dmy[2]) >= 0)) ? "" + (2000 + int.Parse(dmy[2])) : ((int.Parse(dmy[2]) < 100) && (int.Parse(dmy[2]) >= 40)) ? "" + (1900 + int.Parse(dmy[2])) : dmy[2];
                    dmy[1] = int.Parse(dmy[1]) < 10 ? "0" + int.Parse(dmy[1]) : dmy[1];
                    dmy[0] = int.Parse(dmy[0]) < 10 ? "0" + int.Parse(dmy[0]) : dmy[0];
                    return dmy[2] + "-" + dmy[1] + "-" + dmy[0];
                }
                else
                {
                    MessageBox.Show("Sai định dạng ngày tháng");
                    return null;
                }
            }
            MessageBox.Show("Sai định dạng ngày tháng");
            return null;
        }
        public static bool checkFormatDate(string str)
        {
            // full string date
            string[] dmy1 = str.Split('-');
            string[] dmy2 = str.Split('/');
            if(dmy1.Length == 3)
            {
                for(int i=0; i< dmy1.Length; i++)
                {
                    int number;
                    if (int.TryParse(dmy1[i], out number))
                    {
                        if (number < 10)
                        {
                            dmy1[i] = "0" + number;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                int numYear;
                if(int.TryParse(dmy1[2], out numYear))
                {
                    if(numYear < 40 && numYear >= 0)
                    {
                        dmy1[2] = "" + (numYear + 2000);
                    } else if(numYear < 100 && numYear >= 40)
                    {
                        dmy1[2] = "" + (numYear + 1900);
                    }
                }
                str = dmy1[0] + "-" + dmy1[1] + "-" + dmy1[2];
            }
            else if (dmy2.Length == 3)
            {
                for (int i = 0; i < dmy2.Length; i++)
                {
                    int number;
                    if (int.TryParse(dmy2[i], out number))
                    {
                        if (number < 10)
                        {
                            dmy2[i] = "0" + number;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                int numYear;
                if (int.TryParse(dmy2[2], out numYear))
                {
                    if (numYear < 40 && numYear >= 0)
                    {
                        dmy2[2] = "" + (numYear + 2000);
                    }
                    else if (numYear < 100 && numYear >= 40)
                    {
                        dmy2[2] = "" + (numYear + 1900);
                    }
                }
                str = dmy2[0] + "/" + dmy2[1] + "/" + dmy2[2];
            }
            else
            {
                return false;
            }
            // format date dd/mm/yyyy or dd-mm-yyyy
            if (!Regex.IsMatch(str, @"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[012])/(19|20)\d\d$"))
            {
                if (!Regex.IsMatch(str, @"^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[012])-(19|20)\d\d$"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            
            return true;
        }
        public static void updateCompanyByDate(int IDCompany, bool isEveryday = false)
        {
            DateTime today = DateTime.Today;
            int sumExemption = 0, sumAvailable = 0, sumNotYet = 0, sumOther = 0, sum = 0;
            string sqlExemption = "select count(IDCompany) Amount  from Employees where Hidden_flag =0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')"+" and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' and WorkPermit in (0, 3) and IDCompany = "+IDCompany;
            string sqlAvailable = "select count(IDCompany) Amount  from Employees where Hidden_flag =0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')" + " and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' and WorkPermit in (1, 4) and IDCompany = " + IDCompany;
            string sqlNotYet = "select count(IDCompany) Amount  from Employees where Hidden_flag =0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')" + " and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' and WorkPermit in (2, 5) and IDCompany = " + IDCompany;
            string sqlOther = "select count(IDCompany) Amount  from Employees where Hidden_flag =0 and Employees.IDEmployee not in (select IDEmployee from Employees where DateOfLeave <= '" + today.ToString("yyyy-MM-dd") + "')" + " and TemporaryStay >= '" + today.ToString("yyyy-MM-dd") + "' and WorkPermit = 6 and IDCompany = " + IDCompany;
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    // sum exemption
                    adapter.SelectCommand = new SqlCommand(sqlExemption, con);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        sumExemption = int.Parse(dr["Amount"].ToString());
                    }
                    // sum available
                    adapter.SelectCommand = new SqlCommand(sqlAvailable, con);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        sumAvailable = int.Parse(dr["Amount"].ToString());
                    }
                    // sum not yet
                    adapter.SelectCommand = new SqlCommand(sqlNotYet, con);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        sumNotYet = int.Parse(dr["Amount"].ToString());
                    }
                    // sum other
                    adapter.SelectCommand = new SqlCommand(sqlOther, con);
                    adapter.Fill(dt);
                    foreach (DataRow dr in dt.Rows)
                    {
                        sumOther = int.Parse(dr["Amount"].ToString());
                    }
                    // sum
                    sum = sumExemption + sumAvailable + sumNotYet + sumOther;
                    string updateCompany = "";
                    if (isEveryday)
                    {
                        updateCompany = "update Companies set TotalAmount = " + sum + ", AmountOfExemption = " + sumExemption
                        + ", QuantityAvailable = " + sumAvailable + ", QuantityNotYet = " + sumNotYet
                        + ", NumberOfPersonalities = " + sumOther + ", UpdateDay = '" + today.ToString("yyyy-MM-dd") + "' where UpdateDay < '"
                        + today.ToString("yyyy-MM-dd") + "' and IDCompany = " + IDCompany;
                    } else
                    {
                        updateCompany = "update Companies set TotalAmount = " + sum + ", AmountOfExemption = " + sumExemption
                        + ", QuantityAvailable = " + sumAvailable + ", QuantityNotYet = " + sumNotYet
                        + ", NumberOfPersonalities = " + sumOther + " where IDCompany = " + IDCompany;
                    }
                    SqlCommand comm = new SqlCommand(updateCompany, con);
                    comm.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }
            }
        }
        public static bool checkExistInDatabase(string sql)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    con.Close();
                }

            }
        }

        public static bool checkInvestment(int idCompany, string passport)
        {
            if (idCompany == 0 || string.IsNullOrEmpty(passport))
            {
                return false;
            }
            string sql = "select * from Investment where IDCompany = " + idCompany + " and Passport like N'" + passport + "'";
            return checkExistInDatabase(sql);
        }
        public static int countDataInDatebase(string sql)
        {
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(sql, con);
                    adapter.Fill(dt);
                    return dt.Rows.Count;
                }
                catch (Exception ex)
                {
                    return 0;
                }
                finally
                {
                    con.Close();
                }

            }
        }
        public static ObservableCollection<int> getStatistics(ObservableCollection<Employee> listEmployees)
        {
            ObservableCollection<int> listStatistics = new ObservableCollection<int>();
            // work permit
            int available = 0, exemption = 0, notYet = 0, relative = 0, available_invest = 0, exemption_invest = 0, notYet_invest = 0;
            // settlement results
            int ghtt = 0, visa = 0, ttt = 0, other = 0;
            if(listEmployees != null && listEmployees.Count > 0){
                foreach(Employee employee in listEmployees)
                {
                    switch (employee.WorkPermit)
                    {
                        case 0:
                            exemption++;
                            break;
                        case 1:
                            available++;
                            break;
                        case 2:
                            notYet++;
                            break;
                        case 3:
                            exemption_invest++;
                            break;
                        case 4:
                            available_invest++;
                            break;
                        case 5:
                            notYet_invest++;
                            break;
                        case 6:
                            relative++;
                            break;
                    }
                    switch (employee.SettlementResults)
                    {
                        case 0:
                            ghtt++;
                            break;
                        case 1:
                            visa++;
                            break;
                        case 2:
                            ttt++;
                            break;
                        case 3:
                            other++;
                            break;
                    }
                }
            }
            // total
            int total_available = available + available_invest;
            int total_exemption = exemption + exemption_invest;
            int total_notYet = notYet + notYet_invest;
            listStatistics.Add(total_available);
            listStatistics.Add(total_exemption);
            listStatistics.Add(total_notYet);
            listStatistics.Add(relative);

            // settlement results
            listStatistics.Add(ghtt);
            listStatistics.Add(visa);
            listStatistics.Add(ttt);
            listStatistics.Add(other);

            // GPLĐ detals
            listStatistics.Add(available);
            listStatistics.Add(exemption);
            listStatistics.Add(notYet);
            listStatistics.Add(available_invest);
            listStatistics.Add(exemption_invest);
            listStatistics.Add(notYet_invest);
            listStatistics.Add(relative);
            return listStatistics;
        }
        public static ObservableCollection<int> getStatistics(ObservableCollection<Company> listCompanies)
        {
            ObservableCollection<int> listStatistics = new ObservableCollection<int>();
            int sumItem = 0, total = 0, available = 0, exemption = 0, notYet = 0, relative = 0, registration = 0, unregistration = 0;
            if(listCompanies != null && listCompanies.Count > 0)
            {
                sumItem = listCompanies.Count;
                foreach(Company company in listCompanies)
                {
                    total += company.TotalAmount;
                    available += company.QuantityAvailable;
                    exemption += company.AmountOfExemption;
                    notYet += company.QuantityNotYet;
                    relative += company.NumberOfPersonalities;
                    // check registration profile
                    string rp = company.RegistrationProfile;
                    if (String.IsNullOrWhiteSpace(rp))
                    {
                        // registration is null then company unregistration
                        unregistration += 1;
                    }
                    else
                    {
                        rp = rp.ToLower();
                        if (rp.Contains("chưa") || rp.Contains("không"))
                        {
                            unregistration += 1;
                        }
                        else
                        {
                            registration += 1;
                        }
                    }
                }
            }
            listStatistics.Add(sumItem);
            listStatistics.Add(total);
            listStatistics.Add(available);
            listStatistics.Add(exemption);
            listStatistics.Add(notYet);
            listStatistics.Add(relative);
            listStatistics.Add(registration);
            listStatistics.Add(unregistration);
            return listStatistics;
        }
        public static ObservableCollection<CareerGroup> getListCareerGroup()
        {
            ObservableCollection<CareerGroup> listCareerGroup = new ObservableCollection<CareerGroup>();
            string cs = "Data Source=" + Constants.Source + ",1433;Initial Catalog=" + Constants.Catalog + ";Persist Security Info=True; User ID =" + Constants.IDSQLServer + "; Password=" + Constants.PasswordSQLServer + ";";
            using (SqlConnection con = new SqlConnection(cs))
            {
                DataTable dt = new DataTable();
                try
                {
                    con.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand("select * from CareerGroups", con);
                    adapter.Fill(dt);
                    if (dt.Rows.Count > 0)
                    {
                        listCareerGroup.Clear();
                        foreach (DataRow dr in dt.Rows)
                        {
                            CareerGroup rowCareerGroup = new CareerGroup();
                            rowCareerGroup.IDCG = int.Parse(dr["IDCG"].ToString());
                            rowCareerGroup.CareerGroupName = dr["CareerGroupName"].ToString();
                            if(rowCareerGroup.IDCG == 1)
                            {
                                rowCareerGroup.CareerGroupName = " ----- ";
                            }
                            listCareerGroup.Add(rowCareerGroup);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Không có nhóm ngành nghề");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    con.Close();
                }

            }
            return listCareerGroup;
        }
        public static bool checkSettlementResultsString(string srs, out string visanumber, out string cardCreationDate, out string temporaryStay)
        {
            if (string.IsNullOrWhiteSpace(srs))
            {
                visanumber = "";
                cardCreationDate = "";
                temporaryStay = "";
                return false;
            }
            string visaStr = "";
            string[] str = srs.Trim().Split(' ');
            // get visa number
            if (str.Length == 3)
            {
                visaStr = str[1].Trim();
            } else if(str.Length == 2)
            {
                // check string is visa, ghtt, ttt
                if((!"VISA".Equals(str[0].ToUpper().Trim())) && (!"GHTT".Equals(str[0].ToUpper().Trim())) && (!"TTT".Equals(str[0].ToUpper().Trim())))
                {
                    visaStr = str[0].Trim();
                }
            }
            visanumber = visaStr;
            // get date
            string[] strDate = str[str.Length - 1].Trim().Split('-');
            if(strDate.Length == 2 || strDate.Length == 6)
            {
                if(strDate.Length == 2)
                {
                    // dd/MM/yyyy-dd/MM/yyyy
                    if(checkFormatDate(strDate[0]) && checkFormatDate(strDate[1]))
                    {
                        cardCreationDate = strDate[0];
                        temporaryStay = strDate[1];
                        return true;
                    }
                } else
                {
                    // dd-MM-yyyy-dd-MM-yyyy
                    strDate[0] += "-" + strDate[1] + "-" + strDate[2];
                    strDate[3] += "-" + strDate[4] + "-" + strDate[5];
                    if (checkFormatDate(strDate[0]) && checkFormatDate(strDate[3]))
                    {
                        cardCreationDate = strDate[0];
                        temporaryStay = strDate[3];
                        return true;
                    }
                }
            }
            cardCreationDate = "";
            temporaryStay = "";
            return false;
        }
        public static string getSettlementResults(int settlementResults)
        {
            switch(settlementResults)
            {
                case 0:
                    return "GHTT";
                case 1:
                    return "VISA";
                case 2:
                    return "TTT";
                case 3:
                    return "Khác";
                default:
                    return "Khác";
            }
        }
        public static string convertStringOwned(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str.Replace("'", "''").Trim();
            }
            return "";
        }
        public static string ToTitleCase(string str)
        {
            if (!String.IsNullOrWhiteSpace(str))
            {
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
            }
            return "";
        }
        private static readonly string[] VietnameseSigns = new string[]
        {
        "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴÉÈẸẺẼÊẾỀỆỂỄÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠÚÙỤỦŨƯỨỪỰỬỮÍÌỊỈĨĐÝỲỴỶỸ",

        "AS","AF","AJ","AR","AX","AA","AAS","AAF","AAJ","AAR","AAX","AW","AWS","AWF","AWJ","AWR","AWX",
        "ES","EF","EJ","ER","EX","EE","EES","EEF","EEJ","EER","EEX",
        "OS","OF","OJ","OR","OX","OO","OOS","OOF","OOJ","OOR","OOX","OW","OWS","OWF","OWJ","OWR","OWX",
        "US","UF","UJ","UR","UX","UW","UWS","UWF","UWJ","UWR","UWX",
        "IS","IF","IJ","IR","IX",
        "DD",
        "YS","YF","YJ","YR","YX"

        };
        public static string RemoveSign4VietnameseString(string str)
        {
            Trace.WriteLine("-------------------------------------------------------------------");
            Trace.WriteLine(str);
            for (int j = 0; j < VietnameseSigns[0].Length; j++)
            {
                str = str.Replace(VietnameseSigns[0][j].ToString(), VietnameseSigns[j+1]);
            }
            Trace.WriteLine(str);
            Trace.WriteLine("-------------------------------------------------------------------");
            return str;
        }

        public static void CopyFile(string source, string destination, string fileName)
        {
            try
            {
                // Use Path class to manipulate file and directory paths.
                string sourceFile = Path.Combine(source);
                string destFile = Path.Combine(destination, fileName);
                DisconnectFromShare(Constants.PathShare, true);
                ConnectToShare(Constants.PathShare, Constants.AccountShare, Constants.PassShare);
                // To copy a folder's contents to a new location:
                // Create a new target folder, if necessary.
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }

                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                File.Copy(sourceFile, destFile, true);
                DisconnectFromShare(Constants.PathShare, true);
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private static string ConnectToShare(string uri, string username, string password)
        {
            //Create netresource and point it at the share
            NETRESOURCE nr = new NETRESOURCE();
            nr.dwType = RESOURCETYPE_DISK;
            nr.lpRemoteName = uri;
            //Create the share
            int ret = WNetUseConnection(IntPtr.Zero, nr, password, username, 0, null, null, null);

            //Check for errors
            if (ret == NO_ERROR)
                return null;
            else
                return GetError(ret);
        }

        public static string DisconnectFromShare(string uri, bool force)
        {
            //remove the share
            int ret = WNetCancelConnection(uri, force);

            //Check for errors
            if (ret == NO_ERROR)
                return null;
            else
                return GetError(ret);
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        #region P/Invoke Stuff
        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection(
            string lpName,
            bool fForce
            );

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        #region Consts
        const int RESOURCETYPE_DISK = 0x00000001;
        const int CONNECT_UPDATE_PROFILE = 0x00000001;
        #endregion

        #region Errors
        const int NO_ERROR = 0;

        const int ERROR_ACCESS_DENIED = 5;
        const int ERROR_ALREADY_ASSIGNED = 85;
        const int ERROR_BAD_DEVICE = 1200;
        const int ERROR_BAD_NET_NAME = 67;
        const int ERROR_BAD_PROVIDER = 1204;
        const int ERROR_CANCELLED = 1223;
        const int ERROR_EXTENDED_ERROR = 1208;
        const int ERROR_INVALID_ADDRESS = 487;
        const int ERROR_INVALID_PARAMETER = 87;
        const int ERROR_INVALID_PASSWORD = 1216;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_NO_MORE_ITEMS = 259;
        const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        const int ERROR_NO_NETWORK = 1222;
        const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;

        const int ERROR_BAD_PROFILE = 1206;
        const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        const int ERROR_DEVICE_IN_USE = 2404;
        const int ERROR_NOT_CONNECTED = 2250;
        const int ERROR_OPEN_FILES = 2401;

        private struct ErrorClass
        {
            public int num;
            public string message;
            public ErrorClass(int num, string message)
            {
                this.num = num;
                this.message = message;
            }
        }

        private static ErrorClass[] ERROR_LIST = new ErrorClass[] {
            new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
            new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
            new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
            new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
            new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
            new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
            new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
            new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
            new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
            new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
            new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
            new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
            new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
            new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
            new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
            new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
            new ErrorClass(ERROR_SESSION_CREDENTIAL_CONFLICT, "Error: Credential Conflict"),
        };
        private static string GetError(int errNum)
        {
            foreach (ErrorClass er in ERROR_LIST)
            {
                if (er.num == errNum)
                {
                    return er.message;
                }
            }
            return "Error: Unknown, " + errNum;
        }
        #endregion

        #endregion
    }
}
