using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Form1 : Form
    {
        NetworkStream nStream;
        BinaryWriter sendData;
        BinaryReader readData;

        string PlayerName;
        string str;

        bool flag;
        bool winner;
        string win;

        string PlayingChar;
        string OtherPlayingChar;

        List<string> Moves;

        bool Turn;
        bool WinFlag;

        Color m_lineColor;
        Pen m_linePen;
        private int moves;


        //public string CompChar { get; private set; }

        public Form1()
        {
            InitializeComponent();

            flag = false;
            winner = false;

            moves = 1;

            AddEventOnButtons();
            Moves = new List<string>();

            m_lineColor = Color.Black;
            m_linePen = new Pen(m_lineColor, 4);

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            TcpClient client = new TcpClient();
            try
            {
                //client.Connect("127.0.0.1", 49677);


                client.Connect("172.16.3.199", 40000);
               
                nStream = client.GetStream();
                //send data
                sendData = new BinaryWriter(nStream);
                readData = new BinaryReader(nStream);

                if (!String.IsNullOrEmpty(textBox1.Text))
                {
                    PlayerName = textBox1.Text;
                    if (radioButton2.Checked)
                    {
                        flag = true;
                    }
                    try
                    {
                        sendData.Write(PlayerName + " " + flag.ToString().ToLower());

                        panel1.Visible = true;
                        panel1.Paint += new PaintEventHandler(Panel1_Paint);
                        panel1.Refresh();
                        PName.Text = PlayerName;

                        if (flag)
                        {
                            DisabledMood();
                            //read playing character
                            str = readData.ReadString();
                            PlayingChar = str;
                          //  MessageBox.Show("Play char "+PlayingChar);
                            if (str == "X")
                            {
                                OtherPlayingChar = "O";
                            }
                            else
                            {
                                OtherPlayingChar = "X";
                            }
                            //MessageBox.Show("other char " + OtherPlayingChar);
                            ChangeTurn();
                        }
                        else
                        {
                            RecieveResponse();
                        }
                    }
                    catch
                    {
                        LostConnection();
                    }

                }
            }
            catch
            {
                MessageBox.Show("No Connection! Please, Try Again Later");
            }
        }
        private void ChangeTurn()
        {
            try
            {
                WinFlag = readData.ReadBoolean();

                if (!WinFlag && Moves.Count < 10) // !winflag
                {
                    Turn = readData.ReadBoolean();

                    if (Turn == true)
                    {
                        PlayMood();
                    }
                    else
                    {
                        DisabledMood();

                        WinFlag = readData.ReadBoolean();
                        if (!WinFlag && Moves.Count < 10) // !winflag
                        {
                            Turn = readData.ReadBoolean();
                            OtherPlayer();
                            PlayMood();
                        }
                        else
                        {
                            OtherPlayer();
                            ThereIsWinner(); 
                        }
                    }
                }
            }
            catch
            {
                LostConnection();
            }
        }
        public void PlayMood()
        {
            foreach (Control btn in panel1.Controls)
            {
                if (btn.Name.Contains("l"))
                {

                    Button button = (Button)btn;
                    button.Enabled = true;

                    foreach (string s in Moves)
                    {
                        if (btn.Name.Contains(s))
                        {
                            button.Enabled = false;
                        }
                    }
                }
            }
        }

        private void DisabledMood()
        {
            foreach (Control btn in panel1.Controls)
            {
                if (btn.Name.Contains("l"))
                {
                    Button button = (Button)btn;
                    button.Enabled = false;
                }
            }
        }
        private void AddEventOnButtons()
        {
            foreach (Control btn in panel1.Controls)
            {
                if (btn.Name.Contains("l"))
                {
                    Button button = (Button)btn;
                    button.Click += new EventHandler(EventOnClick);
                }
            }
        }
        private void EventOnClick(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = PlayingChar;
            clickedButton.Enabled = false;


            if (flag)
            {
                if (!CheckForWinner() && Moves.Count < 10)
                {
                    Moves.Add(clickedButton.Name[1].ToString());
                    try
                    {
                        sendData.Write(clickedButton.Name[1].ToString()); // send move to server 
                        
                    }
                    catch { LostConnection(); }
                    ChangeTurn();
                }

                else
                {
                    MessageBox.Show(Moves.Count.ToString());

                    try
                    {
                        sendData.Write(clickedButton.Name[1].ToString());
                    }
                    catch { LostConnection(); }
                    ThereIsWinner(); //some modification
                }
            }
            else{
                 if (!CheckForWinner()&& moves < 9)
                {

                    moves++;
                    try
                    {
                        sendData.Write("play" + " " + clickedButton.Name[1]);
                        computerPlayes();
                    }
                    catch
                    {
                        LostConnection();
                    }
                }
                else
                {
                    ThereIsWinner(); //some modification
                }
            }

            //////
            //if (!CheckForWinner() && flag)
            //{
            //    Moves.Add(clickedButton.Name[1].ToString());
            //    try
            //    {
            //        sendData.Write(clickedButton.Name[1].ToString()); // send move to server 

            //        ChangeTurn();
            //    }
            //    catch { LostConnection(); }
            //}
            //else if (!CheckForWinner() && !flag && moves < 9)
            //{

            //    moves++;
            //    try
            //    {
            //        sendData.Write("play" + " " + clickedButton.Name[1]);
            //        computerPlayes();
            //    }
            //    catch
            //    {
            //        LostConnection();
            //    }
            //}
            //else
            //{
            //    //if (flag) {*********************************
            //    try
            //    {
            //        sendData.Write(clickedButton.Name[1].ToString());
            //    }
            //    catch { LostConnection(); }
            //    // }
            //    ThereIsWinner(); //some modification
            //}

        }
        private void OtherPlayer()
        {
            try
            {
                str = readData.ReadString();

                foreach (Control btn in panel1.Controls)
                {
                    if (btn.Name.Contains(str))
                    {
                        Button button = (Button)btn;
                        button.Enabled = false;
                        button.Text = OtherPlayingChar;
                    }
                }
                Moves.Add(str);

                if (WinFlag)
                {
                    ThereIsWinner();
                }
            }
            catch
            {
                LostConnection();
            }
        }
        private void ThereIsWinner()
        {

            foreach (Control btn in panel1.Controls)
            {
                if (btn.Name.Contains("l"))
                {
                    Button button = (Button)btn;
                    button.Enabled = false;
                }
            }
            flag = false;
            CheckForWinner();
           // MessageBox.Show(win);

            if (win == PlayingChar)
            {
                str = "Winner";
            }
            else if (win == OtherPlayingChar )
            {
                str = "Loser";
            }
            else
            {
                str = "No Winner";
            }

            dialog d = new dialog(str);
            DialogResult dResult;
            dResult = d.ShowDialog();

            if (dResult == DialogResult.OK)
            {
                if (!flag)
                {
                    try
                    {
                        sendData.Write("End");
                    }
                    catch
                    {
                        LostConnection();
                    }
                }
                RestartGame();
                EndConnection();
            }
            else
            {
                //sendData.Write(RestartGameFlag);
                if (!flag)
                {
                    try
                    {
                        sendData.Write("end");
                    }
                    catch
                    {
                        LostConnection();
                    }
                }
                EndConnection();
                Close();
            }
        }
        private bool CheckForWinner()
        {
            winner = false;
            Graphics g = panel1.CreateGraphics();

            //horizontal check
            if (l1.Text == l2.Text && l1.Text == l3.Text && !String.IsNullOrEmpty(l1.Text))
            {
                winner = true;
                win = l1.Text;
                g.DrawLine(m_linePen, 241, 167, 598, 167);
            }
            else if (l4.Text == l5.Text && l4.Text == l6.Text && !String.IsNullOrEmpty(l4.Text))
            {
                winner = true;
                win = l4.Text;

                g.DrawLine(m_linePen, 241, 250, 598, 250);
            }
            else if (l7.Text == l8.Text && l7.Text == l9.Text && !String.IsNullOrEmpty(l7.Text))
            {
                winner = true;
                win = l7.Text;

                g.DrawLine(m_linePen, 241, 330, 598, 330);
            }
            // vertical 
            if (l1.Text == l4.Text && l1.Text == l7.Text && !String.IsNullOrEmpty(l1.Text))
            {
                winner = true;
                win = l1.Text;

                g.DrawLine(m_linePen, 277, 145, 277, 343);

            }
            else if (l2.Text == l5.Text && l2.Text == l8.Text && !String.IsNullOrEmpty(l2.Text))
            {
                winner = true;
                win = l2.Text;
                g.DrawLine(m_linePen, 425, 145, 425, 343);
            }
            else if (l3.Text == l6.Text && l3.Text == l9.Text && !String.IsNullOrEmpty(l3.Text))
            {
                winner = true;
                win = l7.Text;
                g.DrawLine(m_linePen, 560, 145, 560, 343);
            }
            //diagonal 
            if (l1.Text == l5.Text && l1.Text == l9.Text && !String.IsNullOrEmpty(l1.Text))
            {
                winner = true;
                win = l1.Text;
                g.DrawLine(m_linePen, 241, 145, 598, 343);
            }
            else if (l3.Text == l5.Text && l3.Text == l7.Text && !String.IsNullOrEmpty(l3.Text))
            {
                winner = true;
                win = l4.Text;

                g.DrawLine(m_linePen, 598, 145, 241, 343);
            }

            if (flag)
            {
                try
                {
                    sendData.Write(winner);
                }
                catch
                {
                    LostConnection();
                }
            }
            return winner;
        }

        private void EndConnection()
        {
            sendData.Close();
            readData.Close();
            nStream.Close();
        }
        private void RestartGame()
        {
            panel1.Visible = false;
            textBox1.Text = PlayerName;
            winner = false;
            win = "";
            Moves.Clear();
            WinFlag = false;
            flag = false;
            moves = 1;
            PlayingChar = "";
            OtherPlayingChar = "";
            foreach (Control btn in panel1.Controls)
            {
                if (btn.Name.Contains("l"))
                {
                    Button button = (Button)btn;
                    button.Text = "";
                    button.Enabled = true;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Text = "Welcome";
        }
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {

            var g = e.Graphics;
            //vertical lines
            g.DrawLine(m_linePen, 350, 145, 350, 343);
            g.DrawLine(m_linePen, 494, 145, 494, 343);
            //horitontal lines
            g.DrawLine(m_linePen, 241, 207, 598, 207);
            g.DrawLine(m_linePen, 241, 292, 598, 292);

        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            Text = "X = " + e.X + " Y = " + e.Y;
        }
        // one Player Functions 
        private void RecieveResponse()
        {
            //readData
            str = readData.ReadString();
            string[] ar = new string[2];
            ar = str.Split(' ');
            PlayingChar = ar[1];
            if (PlayingChar == "X")
            { OtherPlayingChar = "O"; }
            else
            { OtherPlayingChar = "X"; }

            PlayMood();
        }
        private void computerPlayes()
        {
            try
            {
                str = readData.ReadString(); //
                foreach (Control btn in panel1.Controls)
                {
                    if (btn.Name.Contains(str))
                    {
                        Button button = (Button)btn;
                        button.Text = OtherPlayingChar;
                        button.Enabled = false;
                        moves++;
                        if (CheckForWinner())
                        {
                            ThereIsWinner();
                        }
                    }
                }
            }
            catch
            {
                LostConnection();
            }
        }

        private void LostConnection()
        {
            MessageBox.Show("Oops! Connection Lost");
            RestartGame();
            EndConnection();
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            if (!flag)
            {
                try
                {
                    sendData.Write("end");
                }
                catch { LostConnection(); }
            }
            EndConnection();
            Close();
        }
    }
}
