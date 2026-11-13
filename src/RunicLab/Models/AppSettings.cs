using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunicLab.Models;

public class AppSettings
{
    public bool DarkModeEnabled { get; set; }
    public string Benutzername { get; set; } = string.Empty;

    // Du kannst hier beliebig weitere Felder ergänzen
}