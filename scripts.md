# Scripts

## Build Commands
```
$ dotnet build  -p:RunSyntaxHighlighter=false  && dotnet ./bin/Debug/net8.0/portfolio-website.App.dll 127.0.0.1 4000 --dynamic

$ dotnet build && dotnet ./bin/Debug/net8.0/portfolio-website.App.dll 127.0.0.1 4000
```

## Create New Post
```
$ dotnet fsi createNewPost.fsx
```

## CSS Hashing
```
md5 -q portfolio-application/WebRoot/css/index-*.css | cut -c1-8
```
