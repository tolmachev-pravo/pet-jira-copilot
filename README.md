![image](https://user-images.githubusercontent.com/62241382/201462152-67ce6885-2f62-4a57-bccd-8716493dc9f6.png)
![image](https://user-images.githubusercontent.com/62241382/201462143-7dfb8a67-564a-4842-a441-43c76a584170.png)


## Run with docker

1. Buid

```bash
docker build -f .\Dockerfile ./ -t pet-jira:latest
```

2. Run

```bash
docker run -p 5000:80 -d pet-jira:latest 
```