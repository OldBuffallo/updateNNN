using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ReportManager.Bases
{
    class MoneyConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strNum = "" + value;
            return ConvertMoneyToString(strNum);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
        public static string ConvertMoneyToString(string money)
        {
            string strNum = money;
            int startPosition = (strNum.Length % 3 == 0) ? 3 : strNum.Length % 3;
            int numberOfDots = (startPosition == 3) ? strNum.Length / 3 - 1 : strNum.Length / 3;
            for (int i = 0; i < numberOfDots; i++)
            {
                int extraPosition = startPosition + 4 * i;
                strNum = strNum.Substring(0, extraPosition) + "," + strNum.Substring(extraPosition);
            }
            strNum = strNum + "VNĐ";
            return strNum;
        }
    }
}
