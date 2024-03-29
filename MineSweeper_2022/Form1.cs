﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineSweeper_2022
{
    public partial class Form1 : Form
    {
        Bitmap bmp;
        const int W = 37;               // 横のマス
        const int H = 19;               // 縦のマス
        public Form1()
        {
            InitializeComponent();
            //Console.WriteLine(pictureBox1.Width);
            //Console.WriteLine(pictureBox1.Height);
            pictureBox1.Width = W * 22 + 1;
            pictureBox1.Height = H * 22 + 1;
            bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);

            //pictureBoxに，bmpをはりつける
            this.pictureBox1.Image = bmp;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
        }

        const int NCell = W * H;        // すべてのマスの数
        const int NMine = NCell / 10;   // 地雷の数

        public enum MineState   // マスの地雷状態
        {
            IsSafe0 = 0,        // 安全（周辺地雷0個）
            IsSafe1 = 1,        // 安全（周辺地雷1個）
            IsSafe2 = 2,        // 安全（周辺地雷2個）
            IsSafe3 = 3,        // 安全（周辺地雷3個）
            IsSafe4 = 4,        // 安全（周辺地雷4個）
            IsSafe5 = 5,        // 安全（周辺地雷5個）
            IsSafe6 = 6,        // 安全（周辺地雷6個）
            IsSafe7 = 7,        // 安全（周辺地雷7個）
            IsSafe8 = 8,        // 安全（周辺地雷8個）
            IsMine              // 地雷
        };
        public enum OpenState   // マスのオープン状態 
        {
            UnopenNone,         // 未オープン（旗なし）
            UnopenFlag,         // 未オープン（旗あり）
            Opened              // 既オープン
        };

        MineState[,] BoardMine = new MineState[W, H];   // 盤面地雷状態
        OpenState[,] BoardOpen = new OpenState[W, H];   // 盤面オープン状態
        bool IsPeekMode = false;                        // 裏舞台モードか
        bool IsFinished = false;                        // 終了しているか
        int LeftMine = NMine;                           // 残り地雷数
        int LeftOpen = NCell - NMine;                   // 残り安全マス数
        int CursorX = 0;                                // カーソルx座標
        int CursorY = 0;                                // カーソル座標
        int NOpen = 0;                                  // 操作回数
        string Messsage = "";                           // メッセージ
        Random rnd = new Random();                      // 乱数

        MineState CountMine(int X, int Y)                                       // 周辺地雷カウント
        {
            int count = 0;
            if (BoardMine[X,Y] == MineState.IsMine) return MineState.IsMine;    // 地雷なら地雷を返す
            for(int j = Y - 1; j <= Y + 1 ; j++)
            {
                if (j == -1) continue;                  // 端対策
                if (j == H) continue;
                for (int i = X-1; i <= X + 1; i++)
                {
                    if (i == -1) continue;              // 端対策
                    if (i == W) continue;
                    if (i == X && j == Y) continue;                             // そのマスそのものは飛ばす
                    else if (BoardMine[i,j] == MineState.IsMine) count++;       // 地雷ならカウント+1
                }
            }
            return (MineState)Enum.ToObject(typeof(MineState), count);          // 地雷状態を返す
        }

        void InitializeBoard()
        {
            Messsage = "";                              // メッセージ初期化
            IsFinished = false;                         // 終了しているかの初期化
            LeftMine = NMine;                           // 残り地雷数初期化
            LeftOpen = NCell - NMine;                   // 残り安全マス数初期化
            CursorX = 0;                                // カーソルx座標初期化
            CursorY = 0;                                // カーソル座標初期化
            NOpen = 0;                    　             // 操作回数初期化

            //初期配置生成
            {
                var temp = new int[H * W];              // 一次元配列化
                for (int i = 0; i < temp.Length; i++)
                    temp[i] = i;
                for (int i = temp.Length; --i > 0;)
                {
                    var j = rnd.Next(i + 1);            // 乱数で入れ替え
                    (temp[i], temp[j]) = (temp[j], temp[i]);
                }


                for (int i = 0; i < temp.Length; i++)
                {
                    int xx = temp[i] % W;               // 割った余りがx座標
                    int yy = temp[i] / W;               // 商がy座標
                    var s = MineState.IsSafe0;          // すべて周辺地雷0に
                    if (i < NMine)                      // 一次元配列のインデックスが0からNMineまでなら地雷に
                    {
                        s = MineState.IsMine;
                    }
                    BoardOpen[xx, yy] = OpenState.UnopenNone;     // 未オープン旗ナシにする
                    BoardMine[xx, yy] = s;
                }

                for (int j = 0; j < H; j++)
                {
                    for (int i = 0; i < W; i++)
                    {
                        BoardMine[i, j] = CountMine(i, j);   // 周辺地雷数取得
                    }
                }
            }
        }

        void OpenCell(int X, int Y)                         // オープン関数
        {
            if (BoardOpen[X, Y] != OpenState.UnopenNone)    // 旗ナシ未オープンでないならreturn
            {
                return;
            }

            switch (BoardMine[X, Y])
            {
                case MineState.IsSafe0:                         // 周りに地雷が0なら周りを全部オープン
                    BoardOpen[X, Y] = OpenState.Opened;         // オープンして
                    LeftOpen--;                                 // 地雷数を1減らして
                    for (int j = Y - 1; j <= Y + 1; j++)
                    {
                        if (j == -1) continue;                  // 端対策
                        if (j == H) continue;
                        for (int i = X - 1; i <= X + 1; i++)    
                        {
                            if (i == -1) continue;              // 端対策
                            if (i == W) continue;
                            if (BoardOpen[i, j] == OpenState.UnopenNone)    // 周りのマスが旗アリなどの場合はスキップ
                            {
                                OpenCell(i, j);
                            }

                        }
                    }
                    break;
                case MineState.IsMine:                          // 地雷をオープンしたら
                    if(NOpen == 0)
                    {
                        SwapMine(X, Y);
                        OpenCell(X, Y);
                        break;
                    }
                    BoardOpen[X, Y] = OpenState.Opened;         // オープンして
                    LeftMine--;                                 // 地雷数を1減らして
                    IsFinished = true;                          // 終了フラグをたてる
                    Messsage = "残念!";                         // メッセージ
                    break;
                default:
                    BoardOpen[X, Y] = OpenState.Opened;         // それ以外なら1マスだけオープン
                    LeftOpen--;
                    break;

            }
            if (LeftOpen == 0)
            {
                IsFinished = true;               // 全部オープンしたら終了フラグ
                Messsage = "クリア!!";           // メッセージ
            }
            NOpen++;
        }

        void FlagCell(int X, int Y)                         // 旗をたてる
        {
            switch (BoardOpen[X, Y])                        // マスのオープン状態が
            {
                case OpenState.Opened:                      // オープンならreturn
                    break;
                case OpenState.UnopenNone:                  // 旗ナシなら旗をたてる
                    BoardOpen[X, Y] = OpenState.UnopenFlag;
                    LeftMine--;
                    break;
                case OpenState.UnopenFlag:                  // 旗アリなら旗をはずす
                    BoardOpen[X, Y] = OpenState.UnopenNone;
                    LeftMine++;
                    break;
            }
        }


        void drawProcess()                                  // 描画関数
        {
            int dx = pictureBox1.Width / W;
            int dy = pictureBox1.Height / H;

            Graphics g = Graphics.FromImage(bmp);
            Brush gray = new SolidBrush(Color.FromArgb(127, 127, 127));
            Brush black = new SolidBrush(Color.FromArgb(0, 0, 0));
            Brush green = new SolidBrush(Color.FromArgb(103, 224, 77));
            Brush red = new SolidBrush(Color.FromArgb(222, 64, 164));
            Brush white = new SolidBrush(Color.FromArgb(255, 255, 255));
            Pen normalFrame = new Pen(gray, 1);                                // 普通のマス用
            Pen cursorFrame = new Pen(green, 4);                                // カーソル用
            Font font = new Font("BIZ UDゴシック", 12);                         // フォント設定
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Center;                          // X軸中央ぞろえ 
            format.LineAlignment = StringAlignment.Center;                      // Y軸中央ぞろえ

            for (int j = 0; j < H; j++)
            {
                for (int i = 0; i < W; i++)
                {
                    RectangleF rect = new RectangleF(i * dx, j * dy, dx, dy);   // FillRectangleと文字描画のときの範囲指定

                    if (BoardOpen[i, j] == OpenState.Opened && BoardMine[i, j] == MineState.IsMine) 
                        g.FillRectangle(red, rect);                             // オープンされた地雷の場合赤で描画
                    else g.FillRectangle(white, rect);                          // それ以外を白で描画

                    int around = (int)BoardMine[i, j];                          // 周辺の地雷の個数
                    if (BoardOpen[i, j] == OpenState.Opened)                    // オープンされたマス
                    {
                        switch (BoardMine[i, j])
                        {
                            case MineState.IsMine:                              // 地雷なら文字なし
                                break;
                            default:                                            // 安全マスなら黒字で地雷の個数
                                g.DrawString(around.ToString(), font, black, rect, format);
                                break;

                        }
                    }
                    if (BoardOpen[i, j] == OpenState.UnopenFlag)                // フラグありなら赤字でF
                    {
                        g.DrawString("F", font, red, rect, format); 
                    }
                    g.DrawRectangle(normalFrame, i * dx, j * dy, dx, dy);   // グレーで縁取り
                    label1.Text = "残り安全マス: " + LeftOpen.ToString();
                    label2.Text = "残り地雷マス: " + LeftMine.ToString();
                    label3.Text = Messsage;
                }
            }
            g.DrawRectangle(cursorFrame, CursorX * dx, CursorY * dy, dx, dy);   // カーソルがあるところは緑で縁取り

            //pictureBoxの中身を塗り替える
            pictureBox1.Refresh();
        }

        private void SwapMine(int x, int y)             // 地雷と安全マスをいれかえる
        {
            bool in_roop_over = false;                  // 二重ループ終了用
            for (int j = y; j < H; j++)
            {
                for(int i = x; i < W; i++)
                {
                    if (BoardMine[i,j] != MineState.IsMine)
                    {
                        BoardMine[i, j] = MineState.IsMine;
                        BoardMine[x, y] = MineState.IsSafe0;
                        in_roop_over = true;
                        break;
                    }
                }
                if (in_roop_over == true)               // 内側ループが正常終了したら外側も終了
                {
                    break;
                }
            }

            for (int j = 0; j < H; j++)
            {
                for (int i = 0; i < W; i++)
                {
                    BoardMine[i, j] = CountMine(i, j);   // 周辺地雷数取得しなおし
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)  //↑ボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            if (CursorY == 0) CursorY = H - 1;                  // 一番上にいるときは一番下に移動
            else CursorY--;                                     // 一個上のマスに移動
            drawProcess();
            return;
        }

        private void button2_Click(object sender, EventArgs e)  // ←ボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            if (CursorX == 0) CursorX = W - 1;                  // 一番左にいるときは一番右に移動
            else CursorX--;                                     // 一個下のマスに移動
            drawProcess();
            return;
        }

        private void button3_Click(object sender, EventArgs e)  // →ボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            if (CursorX == W - 1) CursorX = 0;                  // 一番右にいるときは一番左に移動
            else CursorX++;                                     // 一個右のマスに移動
            drawProcess();
            return;
        }

        private void button4_Click(object sender, EventArgs e)  // ↓ボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            if (CursorY == H - 1)  CursorY = 0;                  // 一番下にいるときは一番上に移動
            else CursorY++;                                     // 一個下のマスに移動
            drawProcess();
            return;
        }

        private void button5_Click(object sender, EventArgs e)  // オープンボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            OpenCell(CursorX,CursorY);                          // オープン
            drawProcess();                                      // 描画し直し
        }

        private void button6_Click(object sender, EventArgs e)  // フラグボタンの操作
        {
            if (IsFinished == true) return;                     // 終了済ならreturn
            FlagCell(CursorX, CursorY);                         // フラグ
            drawProcess();                                      // 描画し直し
        }

        private void Form1_Load(object sender, EventArgs e)     // ビルドしたときの操作
        {
            InitializeBoard();                                  // 初期化
            drawProcess();                                      // 描画し直し
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)  // リトライボタンの操作
        {
            //if (!IsFinished) return;                            // 誤クリック対策のため、終了していないときは何もしない
            InitializeBoard();                                  // 初期化
            drawProcess();                                      // 描画し直し
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)    // クリックでオープン・フラグ
        {
            if (IsFinished == true) return;                                     // 終了していればそれ以上操作できない
            int dx = pictureBox1.Width / W;                                     // マスのサイズ
            int dy = pictureBox1.Height / H;
            CursorX = e.X / dx;                                                 // カーソル位置変更
            CursorY = e.Y / dy;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    OpenCell(CursorX, CursorY);                                 // 左クリックでオープン
                    break;
                case MouseButtons.Right:
                    FlagCell(CursorX, CursorY);                                 // 右クリックでフラグ
                    break;
            }
            drawProcess();                                                      // 描画しなおし
        }
    }
}
