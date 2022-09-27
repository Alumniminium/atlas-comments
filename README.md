# atlas-comments
cgi app to add support for comments to atlas (and other servers, if i got the cgi interface right)


Put this at the bottom of your article/page

```gemtext
=> /cgi/atlas-comments/rampant-incompetence-among-developers.gmi/view View/Write Comment
```
![comment view](/onpage.webp?raw=true "screenshot")

Click view/write comment to see the comments and add/delete yours.

![comment write](/write.webp?raw=true "screenshot")

put the 'atlas-comments.json' in /etc/atlas/ with the following contents and change the path to where you keep the files you want to enable comments on

```json
{
  "FileSourcePath": "/srv/gemini/her.st/blog/"
}
```

Comments and originals of the articles are stored in the same file.

```json
{
  "FileSourcePath": "/srv/gemini/her.st/blog/",
  "FileComments": {
    "rampant-incompetence-among-developers.gmi"
  },
  "OriginalFiles": {
    "rampant-incompetence-among-developers.gmi": "SG93IG1hb ... RzCg=="
  }
}
```

New comments get written to the end of the file. The original file is stored as base64 encoded string in the json file.