using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;
using Mono.Options;

namespace NetworkAutoLogin
{
    class Program
    {
        static string User;
        static string Password;
        static string AdapterName = "Wi-Fi";
        static string CaptiveUrlPattern = @"\Ahttps?://[^/]*?\.naist\.jp/.+";  // regex
        static string LoginUrl = "https://aruba.naist.jp/cgi-bin/login?cmd=authenticate&user={0}&password={1}";  // regex
        static int Timeout = 5000;  // msec

        static void Main(string[] args)
        {
            // Parse options

            bool help = false;

            OptionSet options = new OptionSet()
            {
                
                { "u|user=",
                    "MANDARA username. (required)",
                    v => User = v },
                { "p|password=",
                    "MANDARA password. (required)",
                    v => Password = v },
                { "a|adapter-name=",
                    string.Format("Wi-Fi adapter name. (default={0})", AdapterName),
                    v => AdapterName = v },
                { "c|captive-url-pattern=",
                    string.Format("Regular expression pattern matching MANDARA Wi-Fi captive portal's URL. (default={0})", CaptiveUrlPattern),
                    v => CaptiveUrlPattern = v },
                { "l|login-url=",
                    "URL used for GET login request. {{0}} and {{1}} represent username and password, respectively." +
                    string.Format(" (default={0})", LoginUrl),
                    v => LoginUrl = v },
                { "t|timeout=",
                    string.Format("HTTP request timeout in msec. (default={0})", Timeout),
                    (int v) => Timeout = v },

                { "h|help", "Show help.", v => help = v != null },
            };

            try
            {
                List<string> extra = options.Parse(args);

                if (help || 0 < extra.Count || User == null || Password == null)
                {
                    options.WriteOptionDescriptions(Console.Out);
                    return;
                }
            }
            catch (OptionException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            CheckConditionAndLogin();
            NetworkChange.NetworkAddressChanged += new NetworkAddressChangedEventHandler(NetworkChange_NetworkAddressChanged);
            Console.ReadKey();
        }

        static void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            CheckConditionAndLogin();
        }

        static void CheckConditionAndLogin()
        {
            if (IsAdapterUp() && IsNetworkCaptive())
            {
                if (Login())
                {
                    Console.WriteLine(DateTime.Now.ToString() + " Login successful.");
                }
                else
                {
                    Console.WriteLine(DateTime.Now.ToString() + " Login failed.");
                }
            }
            Console.WriteLine(DateTime.Now.ToString() + " Condition not satisfied.");
            return;
        }

        static bool IsAdapterUp()
        {
            return NetworkInterface
                .GetAllNetworkInterfaces()
                .Any(a =>
                    (a.Name == AdapterName) &&
                    (a.OperationalStatus == OperationalStatus.Up)
                );
        }

        static bool IsNetworkCaptive()
        {
            string url = "http://connectivitycheck.gstatic.com/generate_204";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = Timeout;
            request.AllowAutoRedirect = false;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.Redirect)
                {
                    string captiveUrl = response.Headers[HttpResponseHeader.Location];
                    return Regex.IsMatch(captiveUrl, CaptiveUrlPattern);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        static bool Login()
        {
            string url = "https://aruba.naist.jp/cgi-bin/login?cmd=authenticate&user=" + User + "&password=" + Password;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = Timeout;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }
    }
}
