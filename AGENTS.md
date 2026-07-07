# AGENTS.md

## Communication

- 默认使用简体中文回复，除非用户明确要求其他语言。
- 代码、命令、变量名、错误信息、日志和文件路径保持原文。
- 结束一次开发任务前，如果改变了项目方向、实验结论、目录约定或实现方案，要同步更新相关文档。

## Project Context

- Project: `OptimizedSpine`
- Unity version: `2022.3.62f2`
- Purpose: 独立 Unity 沙盒，用来实验和验证 spine-unity 的性能优化思路。
- Source discussion: `019f275d-c197-7260-a9a1-c90d907240ab`
- Current stance: 这是低风险的个人实验项目，不是线上生产项目；结论要以本项目中的测量结果为准。

## Core Direction

- 优先顺序：`Benchmark sandbox` -> `Analyzer tool` -> `Runtime LOD`。
- 先建立可重复的测量场景，再写检查/分析工具，最后再做运行时策略。
- 每个优化点都要记录测试条件、指标、结果和结论，避免只凭感觉改代码。
- 涉及 spine-unity 设置或版本行为时，优先查官方文档或当前包源码，再写结论。

## Expected Project Layout

- `Assets/OptimizedSpine/Runtime/`: 运行时代码。
- `Assets/OptimizedSpine/Editor/`: Editor Window、分析器、菜单项等编辑器工具。
- `Assets/OptimizedSpine/Scenes/`: benchmark 和验证场景。
- `Assets/OptimizedSpine/Samples/`: 本项目专用示例资源或占位资源。
- `docs/project-memory.md`: 长期项目记忆和上下文。
- `docs/decisions/`: 重要方向或架构选择记录。
- `docs/experiments/`: 每次性能实验和测量结果记录。

## Working Rules

- 开始任务前先读 `README.md` 和 `docs/project-memory.md`。
- 大任务优先拆分，并在适合时启用 subAgent 模式；小型文档或局部修改可直接完成。
- 不要提交或依赖 Unity 生成目录：`Library/`、`Temp/`、`Logs/`、`UserSettings/`。
- 如果没有明确需求，不要过早引入复杂框架；先让实验可测、可复现、可比较。
- 对 `PMA`、`Straight Alpha`、`Immutable Triangles`、`Use Single Submesh`、`Update When Invisible` 等关键词保持原文，方便搜索和对照文档。
