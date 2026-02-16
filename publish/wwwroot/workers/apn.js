export default {
    async fetch(request, _env) {
        return await handleRequest(request);
    }
}

async function handleRequest(request) {
    let reqHeaders = new Headers(request.headers),
        outBody, outStatus = 200,
        outStatusText = 'OK',
        outCt = null,
        outHeaders = new Headers({
            "Access-Control-Allow-Origin": reqHeaders.get('Origin') || reqHeaders.get('Referer') || "*",
            "Access-Control-Allow-Credentials": "true",
            "Access-Control-Allow-Methods": "GET, OPTIONS",
            "Access-Control-Allow-Headers": reqHeaders.get('Access-Control-Allow-Headers') || "Accept, Authorization, Cache-Control, Content-Type, DNT, If-Modified-Since, Keep-Alive, Origin, User-Agent, X-Requested-With, Token, x-access-token"
        });

    try {
        let url = request.url.substr(8);
        url = url.substr(url.indexOf('/') + 1);

        const uri = new URL(request.url);
        let hostname = 'https://' + uri.hostname;

        if (request.method == "OPTIONS") {
            outStatus = 204;
        } 
        else if (url.indexOf('http:') == -1 && url.indexOf('https:') == -1) {
            outBody = JSON.stringify({
                code: 403,
                usage: 'Host/{URL}'
            });
            outCt = "application/json"; 
            outStatus = 403;
        }
        else 
        {
            url = fixUrl(url);
            const reqUri = new URL(url);
            //let ip = request.headers.get('CF-Connecting-IP');
            //let country = request.headers.get('CF-IPCountry');

            if (url.match(/\.(m3u|ts|m4s|mp4|mkv|aacp|srt|vtt)/)?.length > 0) 
            {
                let fp = {
                    method: request.method,
                    redirect: 'manual',
                    headers: {}
                }

                for (let h of reqHeaders.entries()) { 
                    if (['connection', 'accept-encoding', 'range'].includes(h[0].toLowerCase())) {
                        fp.headers[h[0]] = h[1];
                    }
                }

                fp.headers['user-agent'] = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36';

                let fr = (await fetch(url, fp));
                
                if (fr.status == 301 || fr.status == 302 || fr.status == 303 || fr.status == 307 || fr.status == 308)
                    return Redirect(`${hostname}/${fr.headers.get('location')}`, outHeaders)

                outStatus = fr.status;
                outStatusText = fr.statusText;
                let ctype = fr.headers.get('content-type');
                outBody = (ctype == 'application/x-mpegurl' || ctype == 'application/vnd.apple.mpegurl') ? editm3u(await fr.text(), hostname, url) : fr.body;
				
				for (let h of fr.headers.entries()) { 
                    if (['content-type', 'content-range', 'content-length'].includes(h[0].toLowerCase())) {
                        outHeaders.set(h[0], h[1]);
                    }
                }

                if (url.indexOf('.mp4') >= 0 || url.indexOf('.mkv') >= 0)
                    outHeaders.set('Accept-Ranges', 'bytes');

            } else {
                return Redirect(url, outHeaders);
            }
        }
    } catch (err) {
        outCt = "application/json";
        outBody = JSON.stringify({
            code: -1,
            msg: JSON.stringify(err.stack) || err
        });
        outStatus = 500;
    }

    if (outCt && outCt != "") {
        outHeaders.set("content-type", outCt);
    }

    return new Response(outBody, {
        status: outStatus,
        statusText: outStatusText,
        headers: outHeaders
    })
}

function fixUrl(url) {
    if (url.includes("://")) {
        return url;
    } else if (url.includes(':/')) {
        return url.replace(':/', '://');
    } else {
        return "http://" + url;
    }
}

function Redirect(url, outHeaders) {
    outHeaders.set('location', url);
    return new Response('', {
        status: 302,
        headers: outHeaders
    })
}

function editm3u(_m3u8, hostname, url) {
    let m3u8 = _m3u8.replace(/(https?:\/\/[^\n\r\"\\# ]+)/g, uri => `${hostname}/${uri}`);
    let hlshost = url.match(/(https?:\/\/[^/]+)/)[1];
  
    m3u8 = m3u8.replace(/([\n\r])([^\n\r]+)/g, line => {
      let m = line.match(/([\n\r])([^\n\r]+)/);
      let uri = m[2];
  
      if (uri.includes("#") || uri.includes("\"") || uri.startsWith("http")) {
        return line;
      }
  
      if (uri.startsWith("//")) {
        uri = "https:" + uri;
      } else if (uri.startsWith("/")) {
        uri = hlshost + uri;
      } else {
        return line;
      }
  
      return m[1] + `${hostname}/${uri}`;
    });
  
    m3u8 = m3u8.replace(/(URI=\")([^\"]+)/g, line => {
      let m = line.match(/(URI=\")([^\"]+)/);
      let uri = m[2];
      
      if (uri.includes("\"") || uri.startsWith("http")) {
        return line;
      }
  
      if (uri.startsWith("//")) {
        uri = "https:" + uri;
      } else if (uri.startsWith("/")) {
        uri = hlshost + uri;
      } else {
        return line;
      }
  
      return m[1] + `${hostname}/${uri}`;
    });
  
    return m3u8;
}



function createSignature(ip, url) {
    return crypto.createHash('sha1').update(ip + url + secretKey).digest('hex');
}

function verifySignature(ip, url, receivedHash) {
    return createSignature(ip, url) === receivedHash;
}

