Для изменения настроек, создайте файл в override

sites/beeg.yaml       - оригинал
override/beeg.yaml    - модификация 

В override/beeg.yaml вносите только те параметры которые хотите изменить, host, proxy, etc, остальные параметры будут браться из базового sites/beeg.yaml


=====================================

Допустим вы хотите добавить прокси, для этого создаете override/beeg.yaml с содержимым 

useproxy: true
proxy:
  list:
    - "socks5://127.0.0.1:9050"