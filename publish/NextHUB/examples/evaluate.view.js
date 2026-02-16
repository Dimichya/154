// html, plugin, url

var regex = /<video[^>]+src="([^"]+)"/;
var match = html.match(regex);

if (match)
   return match[1];

return null;