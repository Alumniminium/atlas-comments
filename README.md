# atlas-comments
cgi app to add support for comments to atlas (and other servers, if i got the cgi interface right)


Put this at the bottom of your article/page

```gemtext
=> /cgi/atlas-comments/rampant-incompetence-among-developers.gmi/view View/Write Comment
```
![comment view](/onpage.webp?raw=true "screenshot")

Click view/write comment to see the comments and add/delete yours.

![comment write](/write.webp?raw=true "screenshot")

put the 'atlas-comments.json' in /etc/atlas/ with the following contents and change the path to where you keep the files you want to enable comments on. This is a Key-Value List, so you can add multiple locations.

I store my blog posts in `/srv/gemini/her.st/blog/` and they are accessible at `gemini://her.st/blog/`


```json
"FileSourcePath": {
    "/srv/gemini/her.st/blog/": "blog/",
    "/srv/gemini/her.st/pages/": "pages/"
  },
```

Comments and originals of the articles (in base64) are stored in atlas-comments.json (this is auto generated)

```json
{
  "FileSourcePath": {
    "/srv/gemini/her.st/blog/": "blog/",
    "/srv/gemini/her.st/pages/": "pages/"
  },
  "FileComments": {
    "/srv/gemini/her.st/pages/fsociety.gmi": [
      {
        "Id": "4503895b7d334e8ebc45f9852509822c",
        "File": "/srv/gemini/her.st/pages/fsociety.gmi",
        "Username": "trbl",
        "Thumbprint": "084F2428E4312A2F79F1E19222C088ACF24395B0",
        "Text": "Blackpilled!",
        "TimeStamp": "2022-10-09T19:38:39.7632502+00:00"
      }
    ],
    "/srv/gemini/her.st/blog/rampant-incompetence-among-developers.gmi": [
      {
        "Id": "9342fb087f78476aa65917234b3c98d6",
        "File": "/srv/gemini/her.st/blog/rampant-incompetence-among-developers.gmi",
        "Username": "trbl",
        "Thumbprint": "084F2428E4312A2F79F1E19222C088ACF24395B0",
        "Text": "test comment",
        "TimeStamp": "2022-10-09T19:39:11.9715642+00:00"
      }
    ]
  },
  "OriginalFiles": {
    "/srv/gemini/her.st/pages/fsociety.gmi": "YGBgJCBjYXQgZnNvY2lldHkwMC5kY ... RzCg==",
    "/srv/gemini/her.st/blog/rampant-incompetence-among-developers.gmi": "h78bu9bniby ... RzCg=="
  }
}
```