# PowerToys-Run-WebSearchShortcut

This is a simple [PowerToys Run](https://docs.microsoft.com/en-us/windows/powertoys/run) plugin for quickly selecting a specific search engine to perform searches via keyword prefixes.

## Preview

![Preview of the plugin in action](./ScreenShots/preview.gif)

> If you have configured the [`Keyword`](#keyword) field, you can use a short keyword to specify the search engine.
>
> ![Example of the Keyword field](./ScreenShots/keyword.png)

## Requirements

- PowerToys minimum version 0.79.0

## Installation

- Download the [latest release](https://github.com/Daydreamer-riri/PowerToys-Run-WebSearchShortcut/releases/) by selecting the architecture that matches your machine: `x64` (more common) or `ARM64`
- Close PowerToys (including from the system tray)
- Extract the archive to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`
- Open PowerToys

## Configuration

- Open the config file:

![Open the configuration file with "!config"](./ScreenShots/config.png)

> **Note**: The configuration file is located in `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Settings\Plugins\Community.PowerToys.Run.Plugin.WebSearchShortcut\WebSearchShortcutStorage.json`.

- Inside the config file, you can add your desired search engines. The key is the display name of the search engine, and the [`Url`](#url) property is the URL template for performing the search.

  - You can find an [example](#example) as well as a list of [configuration options](#configuration-fields) below.

- After saving the file, reload the plugin with `!reload`:

![Reload the configuration file with "!reload"](./ScreenShots/reload.png)

### Example

```json
{
  "Google": {
    "Url": "https://www.google.com/search?q=%s",
    "SuggestionProvider": "Google"
  },
  "Bing": {
    "Url": "https://www.bing.com/search?q=%s",
    "SuggestionProvider": "Bing"
  },
  "GitHub": {
    "Url": "https://www.github.com/search?q=%s"
  },
  "GitHubStars": {
    "Url": "https://github.com/stars?q=%s",
    "Keyword": "gs"
  },
  "StackOverflow": {
    "Url": "https://stackoverflow.com/search?q=%s",
    "Keyword": "st"
  },
  "npm": {
    "Url": "https://www.npmjs.com/search?q=%s"
  },
  "YouTube": {
    "Url": "https://www.youtube.com/results?search_query=%s",
    "Keyword": "yt"
  },
  "ChatGPT": {
    "Url": "https://chat.openai.com/?q=%s",
    "Keyword": "gpt"
  }
}
```

## Configuration fields

### `Url`

The URL template for performing the search. Use `%s` as a placeholder for the search query. If the URL does not contain `%s`, the URL will be opened directly when you press enter.

```json
{
  "Google": {
    "Url": "https://www.google.com/search?q=%s"
  }
}
```

> **Note**: To use multiple URLs, you can separate them with a space (" ").
> For example:
>
> ```json
> {
>   "GoogleAndBing": {
>     "Url": "https://www.google.com/search?q=%s https://www.bing.com/search?q=%s"
>   }
> }
> ```
>
> Alternatively, you can use an array of URLs with the [`Urls`](#urls) field.

### `Urls`

`Urls` is an alias for [`Url`](#url), supporting the setting of multiple URLs in an array format.

```json
{
  "GoogleAndBing": {
    "Urls": ["https://www.google.com/search?q=%s", "https://www.bing.com/search?q=%s"]
  }
}
```

### `Keyword`

Used to quickly select the target search engine using a short keyword.

```json
{
  "Google": {
    "Url": "https://www.google.com/search?q=%s",
    "Keyword": "g"
  }
}
```

### `IconUrl`

You can customize the icon by setting this field. Under normal circumstances, you don't need to set this, as the plugin will automatically download the favicon of the website corresponding to the [`Url`](#url) field. However, sometimes you might want to customize the icon, and this field comes in handy.

> **Note**: This field can only be set to a network URL and cannot be set to a local file.

### `IsDefault`

If this option is `true`, the corresponding search engine does not need to input the trigger word.

### `ReplaceWhitespace`

With `ReplaceWhitespace`, you can specify which character(s) to replace a space with when performing a search. This is useful for some websites, such as Wikipedia, which don't use plus signs ("+") to separate words in the URL.

| Value         | Result             |
|---------------|--------------------|
| `" "` or `""` | `Example+search`   |
| `"-"`         | `Example-search`   |
| `"_"`         | `Example_search`   |
| `"+"`         | `Example%2Bsearch` |

> **Note**: As the string is converted to a URL, any spaces in the string (or `ReplaceWhitespace`) will be replaced with plus signs. Any other characters that are not allowed in a URL will be encoded with [percent-encoding](https://en.wikipedia.org/wiki/Percent-encoding).

### `SuggestionProvider`

Used to set the search suggestion provider.

**The currently supported search suggestion providers are:**

- `Google`
- `Bing`
- `npm`
- `CanIUse`

PRs welcome!

> You can also set a Provider to another search engine.
> For example:
>
> ```json
> {
>   "StackOverflow": {
>     "Url": "https://stackoverflow.com/search?q=%s",
>     "SuggestionProvider": "Google"
>   }
> }
> ```

## Credits

- This project can only be completed under the guidance of [this article](https://conductofcode.io/post/creating-custom-powertoys-run-plugins/). Thanks to @hlaueriksson for his great work.

- The search suggestion feature of this project is based on the relevant implementation of [FlowLauncher](https://github.com/Flow-Launcher/Flow.Launcher?tab=readme-ov-file#web-searches--urls), thanks @Flow-Launcher!

## Thanks

Thank you to [@thatgaypigeon](https://github.com/thatgaypigeon) for writing the excellent documentation!

## License

[MIT](./LICENSE) License © 2023 [Riri](https://github.com/Daydreamer-riri)
