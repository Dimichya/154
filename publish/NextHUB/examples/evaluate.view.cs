// html, plugin, url

string src = Regex.Match(html, "<video src=\"([^\"]+)\"").Groups[1].Value;

if (!string.IsNullOrEmpty(src))
    return src;

return null;