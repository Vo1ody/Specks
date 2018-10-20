using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Specks
{
    public partial class Form1 : Form
    {
        private Button[,] btns;
        private int[,] fields;
        public int cl_count = 0;
        ToolTip tt = new ToolTip();
        private TimeSpan ts;
        private ContextMenuStrip cs;

        public Form1()
        {
            InitializeComponent();
            setMenu();
            newGame();
        }

        private void setMenu()
        {
            MenuStrip ms = new MenuStrip();
            Controls.Add(ms);
            MainMenuStrip = ms;
            ToolStripMenuItem file = (ToolStripMenuItem)ms.Items.Add("File");
            ToolStripMenuItem newGame = (ToolStripMenuItem)file.DropDownItems.Add("New Game");
            newGame.Click += newGameToolStripMenuItem_Click;
            newGame.ShortcutKeys = Keys.F2;
            newGame.ShowShortcutKeys = false;
            file.DropDownItems.Add(new ToolStripSeparator());
            ToolStripMenuItem exit = (ToolStripMenuItem)file.DropDownItems.Add("Exit");
            exit.Click += exitToolStripMenuItem_Click;
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            int I, J;
            int dI, dJ;
            GetBtnPosition((Button)sender, out I, out J);
            if (IsMovedBtn(I, J, out dI, out dJ))
            {
                cl_count++;
                if (!toolStripStatusLabel2.Enabled) SetAround(I, J, true);
                fields[I + dI, J + dJ] = fields[I, J];
                fields[I, J] = 0;
                btns[I + dI, J + dJ] = btns[I, J];
                btns[I, J] = null;
                btns[I + dI, J + dJ].Location = new Point(5 + (J + dJ) * 52, 5 + (I + dI) * 52);
                if (!toolStripStatusLabel2.Enabled)
                {
                    SetAround(I+dI, J+dJ, false);
                    tt.SetToolTip(btns[I+dI, J+dJ], GetDirection(-dI, -dJ));
                }
                toolStripStatusLabel1.Text = cl_count.ToString();
            }
            CheckWin();
        }

        private void CheckWin()
        {
            int count;
            bool win = GameWin(out count);
            toolStripProgressBar1.Value = count;
            if (win)
            {
                timer1.Stop();
                SaveScore();
                if (MessageBox.Show("Start new game?", "You win!", MessageBoxButtons.YesNo) == DialogResult.Yes) newGame();
                else Close();
            }
        }

        private void SaveScore()
        {
            List<Score> lst = new List<Score>();
            string[] score = (string[])listBox1.DataSource;
            if(score!=null)
                foreach (string item in score)
                    lst.Add(new Score(item));
            string winner = Interaction.InputBox("Enter your name", "Congratulation, You win!", "No name");
            lst.Add(new Score(){Name = winner, Click = cl_count, Time = (new TimeSpan(0,10,0)).Subtract(ts)});
            lst.Sort(AskScore);
            if(lst.Count > 10)
                lst = lst.GetRange(0, 10);
            string[] tmp = new string[lst.Count];
            for (int i = 0; i < lst.Count; i++)
                tmp[i] = lst[i].ToString();
            File.WriteAllLines("Specks.hs", tmp, Encoding.UTF8);
        }

        private int AskScore(Score x, Score y)
        {
            if (x.Click == y.Click)
                return (x.Time < y.Time ? -1 : (x.Time > y.Time ? 1 : 0));
            return x.Click - y.Click;
        }

        private void SetAround(int I, int J, bool p)
        {
            for (int i = (I == 0 ? 0 : -1); i <= (I == 3 ? 0 : 1); i++)
            {
                for (int j = (J == 0 ? 0 : -1); j <= (J == 3 ? 0 : 1); j++)
                {
                    if (Math.Abs(i + j) != 1) continue;
                    if (btns[I + i, J + j] == null) continue;
                    tt.SetToolTip(btns[I+i, J+j], !p ? "Can't move" : GetDirection(-i, -j));
                }
            }
        }

        private bool GameWin(out int count)
        {
            count = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++, count++)
                {
                    if (i * j == 9 || count > 15) break;
                    if (fields[i, j] != i * 4 + j + 1) return false;
                }
            }
            return true;
        }
        private bool IsMovedBtn(int I, int J, out int dI, out int dJ)
        {
            dI = dJ = 0;
            for (int i = (I == 0 ? 0 : -1); i <= (I == 3 ? 0 : 1); i++)
            {
                for (int j = (J == 0 ? 0 : -1); j <= (J == 3 ? 0 : 1); j++)
                {
                    if (Math.Abs(i + j) != 1) continue;
                    if (fields[I + i, J + j] == 0)
                    {
                        dI = i;
                        dJ = j;
                        return true;
                    }
                }
            }
            return false;
        }
        void GetBtnPosition(Button btn, out int I, out int J)
        {
            I = J = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (btns[i, j] == null) continue;
                    if (btn == btns[i, j])
                    {
                        I = i;
                        J = j;
                        return;
                    }
                }
            }
        }
        private void SetFields(int[,] fields)
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == 3 && j == 3) continue;
                    fields[i, j] = i * 4 + j + 1;
                }
            }
            fields[2, 2] = 12;
            fields[2, 3] = 15;
            fields[3, 2] = 11;
        }

        private void SetRandomFields(int[,] fields)
        {
            Random rnd = new Random();
            Stack<int> n = new Stack<int>(15);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    int tmp;
                    if (i == 3 && j == 3) continue;
                    if (i == 0 && j == 0)
                    {
                        tmp = rnd.Next(15);
                        fields[i, j] = tmp + 1;
                        n.Push(tmp);
                        continue;
                    }
                    do
                    {
                        tmp = rnd.Next(15);
                    } while (n.Contains(tmp));
                    fields[i, j] = tmp + 1;
                    n.Push(tmp);
                }
            }
            fields[3, 3] = 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ts.TotalMilliseconds > 0)
            {
                ts = ts.Subtract(TimeSpan.FromSeconds(1));
                toolStripStatusLabel3.Text = ts.ToString();
            }
            else
            {
                timer1.Stop();
                if (MessageBox.Show("Start new game?", "You lose!", MessageBoxButtons.YesNo) == DialogResult.Yes) newGame();
                else Close();
            }
        }

        private void toolStripStatusLabel2_Click(object sender, EventArgs e)
        {
            ts += TimeSpan.FromSeconds(30);
            toolStripStatusLabel2.Enabled = false;
            SetHelp();
        }

        private void SetHelp()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if(btns[i,j] == null) continue;
                    int dI, dJ;
                    bool moved = IsMovedBtn(i, j, out dI, out dJ);
                    tt.SetToolTip(btns[i,j], !moved?"Can't move":GetDirection(dI,dJ));
                }
            }
        }
        private string GetDirection(int dI, int dJ)
        {
            if (dI == 1) return "Down";
            if (dI == -1) return "Up";
            if (dJ == 1) return "Right";
            return "Left";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
           newGame();
        }

        private void newGame()
        {
            button1.Enabled = true;
            panel1.Enabled = false;
            button2.Enabled = false;
            timer1.Stop();
            panel1.Controls.Clear();
            cl_count = 0;
            ts = new TimeSpan(0, 10, 0);
            toolStripStatusLabel1.Text = "0";
            btns = new Button[4, 4];
            fields = new int[4, 4];
            //SetFields(fields);
            SetRandomFields(fields);

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (i == 3 && j == 3) continue;
                    btns[i, j] = new Button();
                    btns[i, j].Text = fields[i, j].ToString();
                    panel1.Controls.Add(btns[i, j]);
                    btns[i, j].Size = new Size(50, 50);
                    btns[i, j].Location = new Point(5 + j * 52, 5 + i * 52);
                    btns[i, j].Click += Form1_Click;
                    btns[i, j].BackColor = Color.LightSkyBlue;
                    btns[i, j].MouseHover += Form1_MouseHover;
                    btns[i, j].MouseLeave += Form1_MouseLeave; ;
                }
            }
            toolStripStatusLabel3.Text = ts.ToString();
            timer1.Interval = 1000;
            LoadScore();
            CheckWin();
        }

        private void Form1_MouseLeave(object sender, EventArgs e)
        {
            (sender as Button).BackColor = Color.LightSkyBlue;
        }

        private void Form1_MouseHover(object sender, EventArgs e)
        {
            int I, J, dI, dJ;
            GetBtnPosition(sender as Button, out I, out J);
            bool moved = IsMovedBtn(I, J, out dI, out dJ);
            btns[I,J].BackColor = !moved ? Color.Red : Color.Chartreuse;
            //cs = new ContextMenuStrip();
            //ToolStripMenuItem c = (ToolStripMenuItem)cs.Items.Add(!moved ? "Нельзя передвинуть" : GetDirection(dI, dJ));
            //btns[I,J].ContextMenuStrip = cs;
            //c.Click += Form1_Click;
        }
        private void LoadScore()
        {
            try
            {
                listBox1.DataSource = File.ReadAllLines("Specks.hs");
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            timer1.Start();
            panel1.Enabled = true;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Continue")
            {
                timer1.Start();
                panel1.Enabled = true;
                button2.Text = "Pause";
            }
            else
            {
                timer1.Stop();
                panel1.Enabled = false;
                button2.Text = "Continue";
            }
        }
    }
}
