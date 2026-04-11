# Snippy

> A desktop application for managing and executing shell script snippets on remote SSH servers.

Snippy lets you organize your shell scripts as reusable snippets and run them on one or multiple SSH servers simultaneously — with live output streamed directly into the app.

---

## Features

- **Snippet Management** — Create, edit, and organize your shell scripts with a built-in shell editor
- **SSH Host Management** — Manage multiple SSH connections with password or private key authentication
- **Parallel Execution** — Run a snippet on multiple servers at the same time, each with its own live output tab
- **Live Output Streaming** — Watch your script execute in real time via an integrated terminal view
- **Preferences** — Customize terminal font size and application theme (dark/light)
- **Snippet Hub** — Browse and import community snippets directly from within the app with a Metadata URL

---
## Screenshots
### Dark Mode
<div align="center">
  <img src="Assets/Screenshots/snippy_main.png" width="49%" />
  <img src="Assets/Screenshots/snippy_snippetmanagement.png" width="49%" />
  <img src="Assets/Screenshots/snippy_hostmanagement.png" width="49%" />
  <img src="Assets/Screenshots/snippy_execution.png" width="49%" />
</div>

### Light Mode
<div align="center">
  <img src="Assets/Screenshots/snippy_snippetmanagement_light.png" width="49%" />
  <img src="Assets/Screenshots/snippy_hostmanagement_light.png" width="49%" />
  <img src="Assets/Screenshots/snippy_execution_light.png" width="49%" />
</div>

---

## Installation

1. Get your preferred version of Snippy from the releases page
2. Extract the ZIP File
3. Start the 'Snippy' File

**Requirements:**
- Linux / Windows / macOS

---

## Documentation

### Automatic Snippet Parameters

`{{SNIPPY_HOST}} - Represents the current "Host IP" on which the snippet is executed`

`{{SNIPPY_PORT}} - Represents the current "Host Port" on which the snippet is executed` 

`{{SNIPPY_USER}} - Represents the current "Host User" on which the snippet is executed`

`{{SNIPPY_DATE}} - Represents the current Client Date in: YYYY-MM-DD e.g.: 2025-04-11`

`{{SNIPPY_TIME}} - Represents the current Client Time in: HH:MM:SS e.g.: 14:32:05`

`{{SNIPPY_TIMESTAMP}} - Represents the current Client Unix Timestamp. E.g.: 1744380725`

`{{SNIPPY_NAME}} - Represents the snippets name`

`{{SNIPPY_ID}} - Represents the snippets ID`

### Snippet-Hub
The snippet hub is a repository named [**Snippy-Hub**](https://github.com/001Sarper/Snippy-Hub). It is a community-driven hub for sharing shell script snippets for Snippy.
If you want to use or contribute to it, follow its [**Instructions**](https://github.com/001Sarper/Snippy-Hub#how-to-use).

If there are no active contributors, unfortunately the repository also won't grow over time. 



---


## Built With

- [Avalonia UI](https://avaloniaui.net/) — Cross-platform UI framework for .NET
- [SSH.NET](https://github.com/sshnet/SSH.NET) — SSH connectivity and command execution
- [xterm.js](https://xtermjs.org/) — Terminal emulator for live output rendering

---

## Roadmap

- [x] Snippet management with shell editor
- [x] SSH connection management
- [x] Parallel execution on multiple servers
- [x] Live output streaming per server tab
- [x] Preferences window
- [x] Snippet Hub — community snippet sharing via URL
---
## **Create new Feature requests under the Issues Tab**!

---

## License

Snippy is licensed under the [GNU General Public License v3.0](LICENSE).

---

## Author

Made by [@sarp_03](https://github.com/001Sarper)