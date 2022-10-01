using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

public class DevcadeClient
{
	public DevcadeClient()
	{

	}

	public void runGame(string game)
	{
        string myPath = "C:\\Users\\dingus\\Downloads\\publish_noah_windoze\\publish\\BankShot.exe";
        Process process = new Process()
        {
            StartInfo = new ProcessStartInfo(myPath, "{Arguments If Needed}")
            {
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = Path.GetDirectoryName(myPath)
            }
        };

        process.Start();
    }
}
