using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIFC3
{
    public static class Constantes
    {
        public static string key = "48BEDE9BA82C_A4192C9A446D0CE284A5059E90Bd814E46BB280426";
        public static readonly SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constantes.key));
    }
}
