# Bonus.SourceGen

[![CI](https://github.com/Bonuspunkt/Bonus.SourceGen/actions/workflows/ci-build.yml/badge.svg)](https://github.com/Bonuspunkt/Bonus.SourceGen/actions/workflows/ci-build.yml)

see `stories` for design decisions

## Support
|                                      | net48 | net6 | net7 |
|--------------------------------------|---|---|---|
| `[RegisterDelegate]`                 | ☑ | ☑ | ☑ |
| `[UseHistogram(nameof(...))]`        | ☑ | ☑ | ☑ |
| `.RegisterTypeWithDelegateModule<T>` | ☑* | ☑* | ☑ |

\* uses Reflection

### resources used / big thanks to
- https://andrewlock.net/series/creating-a-source-generator/
- https://github.com/ViktorHofer/PackAsAnalyzer
