# Pencil UGUI

Pencil UGUI 是一个把 AI 画布工具接到 Unity UGUI 的桥接项目。它让 Agent 先在本地画布里生成或修改 UI，再把结果导入到 Unity 现有的 `Canvas` 下面。

当前默认画布来源是 `[open-pencil/open-pencil](https://github.com/open-pencil/open-pencil)`：`.fig` 设计文件会先导出为 UI IR JSON，再由 Unity 插件生成 UGUI 层级。

## 适合做什么

- 快速生成游戏 UI 原型，比如设置面板、确认弹窗、商店卡片、任务列表和主菜单局部。
- 让 Cursor、Codex、Claude、Qoder 等 Agent 根据描述创建 UI，并直接放进 Unity。
- 反复修改同一个设计并重新导入，已有 GameObject 会按稳定节点 ID 更新，而不是重复创建。
- 把 AI 生成的画布结果落到原生 UGUI 上，方便继续接 `Button`、`Image`、`TextMeshProUGUI` 和项目脚本。

当前 MVP 更适合结构清楚的 2D 面板：层级、矩形、纯色背景、文本和基础按钮。复杂矢量、阴影、渐变、完整响应式布局和精确视觉还原还不是重点。

## 基本流程

```text
AI / Agent
  -> 创建或更新 OpenPencil .fig
  -> 导出 UI IR JSON
  -> 导入 Unity UGUI Canvas
```

导入器会保留源节点 ID。重新导入时，像 `Confirm Button [0:15]` 这样的对象会被更新，而不是再生成一个新的对象。

## 安装

### 1. 安装 Unity Package

在 Unity Package Manager 中选择 `Add package from git URL...`，填入本仓库的 package 路径：

```text
<this-repo-git-url>?path=/packages/com.jupiterthewarlock.pencil-ugui
```

本地开发时也可以选择 `Add package from disk...`，然后指向：

```text
packages/com.jupiterthewarlock.pencil-ugui/package.json
```

包名是 `Pencil UGUI`，最低 Unity 版本是 `2021.3`，依赖 Unity UGUI 和 TextMeshPro。

### 2. 准备 OpenPencil

当前默认 provider 是 `open-pencil`，需要本机有一个可运行的 `open-pencil/open-pencil` checkout。稍后在 Setup 窗口里把 `openPencilDir` 指到这个目录。

### 3. 完成 Unity Setup

在 Unity 菜单打开：

```text
Tools > Pencil UGUI > Setup...
```

然后按顺序操作：

1. 设置 `openPencilDir`。
2. 点击 `Save Config`。
3. 点击 `Start Server`。
4. 选择要安装 skill 的 Agent 目标。
5. 点击 `Install / Update Skill`。
6. 点击 `Run Doctor` 检查环境。

插件会在 Unity 项目根目录创建 `.pencil-ugui/`，里面保存配置、生成结果和本地工具。Agent 后续会从这里读取项目配置。

## 使用方式

### 让 Agent 生成并导入

完成 Setup 和 skill 安装后，在 Unity 项目根目录启动 Agent，然后描述你想要的 UI：

```text
用 Pencil UGUI 创建一个手机游戏设置面板，包含标题、背景卡片、音乐开关、音效开关、语言下拉框、关闭按钮和确认按钮，并导入到当前选中的 Canvas。
```

Agent 会读取 `.pencil-ugui/config.json`，使用 OpenPencil 创建或更新 `.fig`，导出 UI IR，再导入到 Unity 当前选中的 `Canvas`。

### 手动导出和导入

如果你已经有一个 OpenPencil `.fig` 文件，可以从 Unity 项目根目录运行：

```powershell
node .pencil-ugui/tools/pencil-ugui-cli/bin/pencil-ugui.mjs export `
  --input path/to/design.fig `
  --output .pencil-ugui/generated/panel.json
```

然后在 Unity Hierarchy 里选中目标 `Canvas`，再导入：

```powershell
node .pencil-ugui/tools/pencil-ugui-cli/bin/pencil-ugui.mjs import `
  --ui-ir .pencil-ugui/generated/panel.json `
  --target selection
```

也可以从 Unity 菜单手动选择 UI IR：

```text
Tools > Pencil UGUI > Import UI IR...
```

### 检查环境

如果导出或导入不工作，优先在 Setup 窗口点击 `Run Doctor`。CLI 也有同样入口：

```powershell
node .pencil-ugui/tools/pencil-ugui-cli/bin/pencil-ugui.mjs doctor
```

## 当前支持

- OpenPencil 页面和节点层级。
- 稳定节点 ID、节点名、位置和尺寸。
- `FRAME` 到 `GameObject + RectTransform`，有纯色填充时加 `Image`。
- `RECTANGLE` 到 UGUI `Image`。
- `TEXT` 到 `TextMeshProUGUI`。
- 基础文本内容、字号和颜色。
- 名字里带 `Button` 的节点可以映射到 Unity `Button`。

暂缓范围：PNG 自动导出、复杂矢量、渐变、阴影、模糊、组件变体、复杂 prefab 目标和完整自适应布局。

## 示例

仓库里有一组设置面板样例：

```text
samples/harness/open-pencil-settings-panel.fig
samples/harness/open-pencil-settings-panel.svg
samples/ui-ir/settings-panel.json
```

它用来验证最小闭环：生成一个可识别的设置面板，导入 Unity，并且第二次导入不会重复创建对象。

## 架构和技术栈

Pencil UGUI 的核心边界是 UI IR JSON：

```text
canvas provider -> source exporter -> UI IR JSON -> Unity UGUI importer
```

Unity 导入器只依赖 UI IR，不直接绑定 OpenPencil、`.fig` 文件格式或某个 Agent/MCP 协议。以后要接其他画布工具，只需要新增对应 exporter。

主要组成：

- Unity Editor package：Setup、配置、skill 安装、本地服务和 UGUI 导入。
- Node.js CLI：`doctor`、`export`、`import`、`prompt-context`。
- OpenPencil：当前默认画布 provider。
- UGUI + TextMeshPro：最终生成的 Unity UI。

