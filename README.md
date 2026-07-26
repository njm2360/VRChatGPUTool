# VRChatGPUTool

[![CI](https://github.com/njm2360/VRChatGputool/actions/workflows/ci.yml/badge.svg)](https://github.com/njm2360/VRChatGputool/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/njm2360/VRChatGputool/graph/badge.svg)](https://codecov.io/gh/njm2360/VRChatGputool)
[![Release](https://img.shields.io/github/v/release/njm2360/VRChatGputool)](https://github.com/njm2360/VRChatGputool/releases/latest)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%2F%2011-0078D4)](https://github.com/njm2360/VRChatGputool)
[![License](https://img.shields.io/github/license/njm2360/VRChatGputool)](LICENSE)

VRChatを起動したまま寝落ちしてしまう人のためのGPU電力制限ツールです。指定した時間帯になったら、nvidia-smi経由でGPUに電力制限をかけて電気代を抑えます。

## ダウンロード

[Releases](https://github.com/njm2360/VRChatGputool/releases/latest) からインストーラー(`VRChatGPUTool-vX.Y.Z-setup.exe`)をダウンロードしてください。

## 機能

- **スケジュール制限**: 曜日と時間帯を指定して電力制限をかけます。スロットは複数登録でき、日をまたぐ時間帯(例: 23:00から翌7:00)にも対応しています。
- **寝落ちの自動検出**: 直近5分間のGPU使用率の変動がしきい値未満に収まると、寝ていると判断して自動で制限をかけます。GPU使用率が常時100%に張り付く環境では検出できません。
- **コアクロック制限**: 電力制限に加えてコアクロックの上限も設定できます。下げすぎるとSteamVRがまともに動かなくなるので注意してください。
- **制限解除時の動作選択**: GPUのデフォルト値に戻すか、指定したワット数に戻すかを選べます。
- **消費電力ログと電気代の集計**: GPUの消費電力を記録して、1時間ごとや1日ごとの電力量を確認できます。時間帯別の電気料金単価を設定すれば電気代も出せます。CSVエクスポートにも対応しています。

## 動作環境

- Windows 10 / 11 (x64)
- NVIDIA GPU (nvidia-smiを含むドライバがインストールされていること)
  - Laptop GPUでは正しく動作しません
- [.NET Desktop Runtime 10](https://dotnet.microsoft.com/download/dotnet/10.0)

電力制限の変更自体には管理者権限が必要ですが、インストール時に登録されるWindowsサービス(NvidiaSmiProxy)が代行するため、アプリは通常のユーザー権限で動きます。起動のたびにUACが出ることはありません。(非管理者権限のXSOverlayから操作できます)

## インストール

1. インストーラーを実行します。サービスの登録があるため管理者権限を求められます。
2. 画面の指示に従ってインストールします。完了時にNvidiaSmiProxyサービスが自動で登録、起動されます。
3. スタートメニューまたはデスクトップのショートカットからアプリを起動します。

アンインストールはWindowsの設定から行えます。サービスも一緒に削除されます。

## プロジェクト構成

| プロジェクト      | 内容                                                                         |
| ----------------- | ---------------------------------------------------------------------------- |
| VRCGPUTool        | WPFアプリ本体                                                                |
| NvidiaSmiProxy    | LocalSystemで動くWindowsサービス。名前付きパイプ経由でnvidia-smiを実行します |
| VRCGPUTool.Shared | アプリとサービス間のパイプ通信の定義                                         |
| VRCGPUTool.Tests  | xUnitによるユニットテスト                                                    |

## ビルド

必要なもの:

- .NET SDK 10
- Inno Setup 6 (インストーラーを作る場合)
- ReportGenerator (カバレッジレポートを見る場合)

```sh
make build      # アプリとサービスのビルド
make test       # ユニットテスト
make coverage   # カバレッジレポートの生成と表示
make run        # ビルドして両方を起動
make installer  # publishしてインストーラーを作成
```

makeがない環境では、Makefile内の各コマンドを直接実行してください。

## ライセンス

[BSD 2-Clause License](LICENSE)
