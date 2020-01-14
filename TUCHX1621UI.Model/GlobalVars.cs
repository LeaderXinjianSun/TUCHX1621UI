using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TUCHX1621UI.Model
{
    public static class GlobalVars
    {
        public static Fx5u Fx5u_left, Fx5u_mid;
        public static string GetBanci()
        {
            string rs = "";
            if (DateTime.Now.Hour >= 8 && DateTime.Now.Hour < 20)
            {
                rs += DateTime.Now.ToString("yyyyMMdd") + "Day";
            }
            else
            {
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 8)
                {
                    rs += DateTime.Now.AddDays(-1).ToString("yyyyMMdd") + "Night";
                }
                else
                {
                    rs += DateTime.Now.ToString("yyyyMMdd") + "Night";
                }
            }
            return rs;
        }
    }
}
