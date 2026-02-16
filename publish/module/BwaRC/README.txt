Мод позволяет пользователю привязать свой аккаунт filmix, kinopub, rezka, vokino через IP:9118/kit 

1. Включите в manifest.json
	enable true

2. Перезагрузите lampac
	service lampac restart

3. Включите в init.conf
"kit": {
  "enable": true,
  "path": "module/BwaRC/raw"
},
"KinoPub": {
  "rhub": false,         // запросы через сервер
  "rhub_fallback": true  // разрешить запросы через сервер
},
"Filmix": {
  "rhub": false,
  "rhub_fallback": true
}



---

"Mirage": {
  "kit": false // запретить пользовательские настройки
}

---


Пользовательские настройки имеют некоторые ограничения
* Нельзя использовать сервер как streamproxy 
* Все запросы на стороне клиента через rhub, исключение балансеры с разрешённым rhub_fallback

