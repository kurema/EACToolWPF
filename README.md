# EAC Tool
高機能リッピングツール、EAC (Exact Audio Copy)に対する補助ツール。

# 機能
* URLによる画像ダウンロード→カバーにドロップ
* ドロップした画像を自動でリサイズ&JPEG保存
* 正規表現の自動適用でトラック名を作成
* エラーが残るトラックをメモするファイルの作成 (ドラッグアンドドロップで配置)
* 2枚組CDとかで便利な一時メモ

# スクリーンショット
![image](https://github.com/kurema/EACToolWPF/assets/10556974/20f0d7cc-f03d-4a2d-820c-a631dc57f0dc)
![image](https://github.com/kurema/EACToolWPF/assets/10556974/35f330ef-971d-4344-803a-d247d7299d1b)
![image](https://github.com/kurema/EACToolWPF/assets/10556974/7499df40-5a0d-4b79-aa4c-7f49d948a9ab)

# なぜ？
EACはビットアキュレートでリッピングできる強力なツールなのですが、個人的にちょっと困ることがあります。例えば以下のような。

* カバー画像に使える画像形式が限られている。例えばwebpは使えない。確かPNGも。
* 巨大なカバー画像を取得した場合はリサイズしないと落ちることがある。
* アルバム情報が取得できないCDがある。その場合は検索結果を使うのだが、タイトル一覧に"1."とかがあることが多い。
* エラーが残るトラックをメモする時に"Error.04.txt"みたいなファイルを作成する
* 実行中にWindows全体がフリーズしたら重くなることがある。

最後の理由もあり私はリッピングは専用のPCをリモートデスクトップで使っているのですが、URLコピペが便利なのでそれを想定したツールになりました。
私以外にはあんまり役に立たないツールでしょうが、ともかく公開しておきます。
「こういうツールをいつか作りたいなぁ」と長いこと思っていたのですが、最近似たような別のツールを作った機会に作りました。

