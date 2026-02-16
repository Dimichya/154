// host, init, pl, html, row

string src = Regex.Match(row, "<video src=\"([^\"]+)\"").Groups[1].Value;
if (string.IsNullOrEmpty(src))
    return null;

pl.video = src;
pl.json = false;
return pl;