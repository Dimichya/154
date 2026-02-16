(function () {
  'use strict';

  function _typeof(obj) {
    "@babel/helpers - typeof";

    return _typeof = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function (obj) {
      return typeof obj;
    } : function (obj) {
      return obj && "function" == typeof Symbol && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj;
    }, _typeof(obj);
  }
  
  var plugins = ['s.js', 'o.js'];
  
  if (typeof window.bwainit == 'undefined' && typeof WebAssembly !== 'undefined') 
  {
    window.bwainit = true;
	
    var s = document.createElement('script');
    s.onload = function () 
    {
      if (typeof Blazor == 'undefined'){
        window.bwainit = false;
        return;
	  }
		
      Blazor.start({
        loadBootResource: function loadBootResource(type, name, defaultUri, integrity){
          return '/bwa/_framework/' + name + '?v=' + Math.random();
        }
      }).then(function () 
      {
        var net = new Lampa.Reguest();
        window.httpReq = function (url, post, params) {
          return new Promise(function (resolve, reject) {
            net["native"](url, function (result) {
              if (_typeof(result) == 'object') resolve(JSON.stringify(result));else resolve(result);
            }, reject, post, params);
          });
        };
        var check = function check(good) 
        {
          var initial = false;
          try {
            DotNet.invokeMethodAsync("JinEnergy", 'initial');
            initial = true;
          } catch (e) {}
			
          if (initial) 
          {
            console.log('BWA', 'check cors:', good);
            var type = Lampa.Platform.is('android') ? 'apk' : good ? 'cors' : 'web';
            var conf = '/bwa/settings/' + type + '.json';
            DotNet.invokeMethodAsync("JinEnergy", 'oninit', type, conf);
            Lampa.Utils.putScriptAsync(plugins.map(function (u) {
              return '/bwa/plugins/' + u + '?v=' + Math.random();
            }), function () {});
          }
          else {
            window.bwainit = false;
          }
        };
        if (Lampa.Platform.is('android') || Lampa.Platform.is('tizen')) check(true);else {
          net.silent('https://github.com/', function () {
            check(true);
          }, function () {
            check(false);
          }, false, {
            dataType: 'text'
          });
        }
      })["catch"](function (e) {
        console.log('BWA', 'error:', e);
        window.bwainit = false;
      });
    };
    s.setAttribute('autostart', 'false');
    s.setAttribute('src', '/bwa/_framework/blazor.webassembly.js' + '?v='+Math.random());
    document.body.appendChild(s);
  }

})();