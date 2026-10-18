# Game-demo

## 简要说明
- 一个用于原型开发的 Unity first-person 恐怖灰盒（graybox）演示工程，包含关卡生成器、基本角色控制、拾取物（钥匙）、锁门/逃生门和一个基于视线行为的怪物 AI。

## 主要功能
- HospitalSceneBuilder：一键生成走廊与房间的灰盒场景（使用 Unity 原始体）。
- BasicFirstPersonController：WASD + 鼠标视角的第一人称控制器，含手电筒支持。
- MonsterWatcherAI：当玩家看向怪物时怪物停下，否则追向玩家（注重视线检测）。
- 物品与交互：钥匙拾取、需要钥匙的门、逃生门（按 E 交互）。
- 中文使用说明：Assets/Docs/SetupGuide.md（描述如何搭建与运行场景）。

## 需求（建议）
- Unity 编辑器（建议使用 LTS 版本，例如 2020.3 / 2021.3 / 2022.x LTS）。
- 支持 C# 脚本编译与运行的开发环境。

## 快速开始
1. 克隆仓库：
   git clone https://github.com/bookid284-art/Game-demo.git
2. 用 Unity 打开仓库根目录（或 .sln/.unityproj）。
3. 在 Editor 中打开任意场景或创建空场景，添加一个空的 GameObject 并将 HospitalSceneBuilder 组件附上（参见 Assets/Scripts）。
4. 在该组件的菜单中选择 “Build Hospital Graybox” 构建演示场景。
5. 点击 Play 测试：使用 WASD + 鼠标移动，按 E 与钥匙/门交互。

## 项目结构（示例）
- Assets/Scripts/ - 核心脚本（HospitalSceneBuilder、BasicFirstPersonController、MonsterWatcherAI、KeyPickup、DoorController、EscapeDoor、PlayerInventory）
- Assets/Docs/ - 使用说明与设置指南（中文）
- ProjectSettings/ - Unity 项目配置

## 已知问题与建议
- 目前没有自动化或单元测试，建议在进一步开发时添加 PlayMode/Editor 测试。
- AI、物理与交互逻辑是原型实现，适合改造为更复杂的系统。

## 贡献
- 欢迎提交 issue 或 PR。请在提交前在本地测试并附上复现步骤或改动说明。

## 许可证
- 如果你没有指定许可证，仓库默认无许可证（请根据需要添加 LICENSE 文件，例如 MIT）。
