using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace server2
{
    // public enum PlayerType { SinglePlayer, PlayerOne, PlayerTwo }
    class Client
    {
        public string PlayerName { get; set; }

        public int ClientID { get; set; }

        public List<string> Moves { get; set; }
        public bool EndGame { get; set; }

        //public PlayerType Player { get; set; }

        public NetworkStream nStream { get; set; }
        public BinaryReader readData { get; set; }
        public BinaryWriter sendData { get; set; }

        //public bool RestartGameFlag { get; set; }

        //static Client()
        //{
        //    ClientID = 1;
        //}
        public Client()
        {

            Moves = new List<string>();

            EndGame = false;

            //RestartGameFlag = false;       
        }
    }

    class Server
    {

        TcpListener server;
        IPAddress localaddr;
        byte[] bt;

        public string str { get; set; }
        //bool Turn;

        static bool PlayerOne;
        static bool PlayerTwo;

        List<Client> PlayersList;
        List<Thread> ClientThreads;
        bool DummyFlag;


        int index;

        string FirstChar;
        string SecondChar;
        public Server()
        {
            //bt = new byte[] { 127, 0, 0, 1 };
            bt = new byte[] { 172,16,3,199 };
            localaddr = new IPAddress(bt);
            server = new TcpListener(localaddr, 40000);

         //   Turn = true;
            PlayerOne = false;
            PlayerTwo = false;
            PlayersList = new List<Client>();
            ClientThreads = new List<Thread>();

            //Moves = new List<string>();
            index = 0;
            FirstChar = "X";
            SecondChar = "O";

        }
        static Server()
        {
            PlayerOne = false;
            PlayerTwo = false;
        }
        public void EstablishConnection()
        {
            while (true)
            {
                server.Start();

                Console.WriteLine("Server waiting for connection");

                Socket connection;

                connection = server.AcceptSocket();

                // Console.WriteLine("Client {0} Connected ", Client.ClientID);

                Thread ClientThread = new Thread(() => ClientConnection(connection));
                ClientThreads.Add(ClientThread);
                ClientThread.Start();
            }
        }

        private void ClientConnection(Socket connection)
        {
            Client Player = new Client();
            Player.nStream = new NetworkStream(connection);
            Player.readData = new BinaryReader(Player.nStream);
            Player.sendData = new BinaryWriter(Player.nStream);
            try
            {
                string str = Player.readData.ReadString();
                string[] ar;
                ar = str.Split(' ');

                Console.WriteLine(str);
                //Client.ClientID++;

                if (ar[1] == "false")
                {
                    StartGameOnePlayer(Player);
                }
                else
                {
                    Player.ClientID = index;
                    index++;
                    if (!PlayerOne)
                    {
                        PlayerOne = true;
                        PlayersList.Add(Player);
                        DummyFun();
                    }
                    else
                    {
                        PlayerTwo = true;
                        PlayersList.Add(Player);
                        PlayerOne = PlayerTwo = false;
                        Console.WriteLine(index);
                        StartGame(PlayersList[index - 2].ClientID, PlayersList[index - 1].ClientID);
                    }
                }
            }
            catch
            {

                EndConnection(Player.sendData, Player.readData, Player.nStream);
            }
        }
        public void DummyFun()
        {
            DummyFlag = true;
            while (DummyFlag)
            {
                if (PlayerTwo)
                {
                    DummyFlag = false;
                }
            }
        }

        public void StartGame(int i, int j)
        {

           // List<string> Moves;

            Console.WriteLine("Start Game function");

            List<string> Moves = new List<string>();

            bool Turn = true; // 
            int Dummy = 1;
            //  string WinFlag = "start";
            bool winner = false;

            try
            {
                PlayersList[i].sendData.Write(FirstChar);
                PlayersList[j].sendData.Write(SecondChar);

                Swap();
                while (!winner && Moves.Count < 10)
                {

                    PlayersList[i].sendData.Write(winner);
                    PlayersList[j].sendData.Write(winner);

                    PlayersList[i].sendData.Write(Turn);
                    PlayersList[j].sendData.Write(!Turn);

                    ///////**
                    if (Turn)
                    {
                        if (Dummy != 1)
                        {
                            PlayersList[i].sendData.Write(str);
                            Console.WriteLine(str);
                        }
                        winner = PlayersList[i].readData.ReadBoolean();
                        str = PlayersList[i].readData.ReadString(); // add one new move
                        Console.WriteLine(str);
                        Console.WriteLine(winner.ToString());
                        Turn = false;
                        Dummy = 2;
                    }
                    else
                    {
                        PlayersList[j].sendData.Write(str);
                        Console.WriteLine(str);
                        winner = PlayersList[j].readData.ReadBoolean();
                        str = PlayersList[j].readData.ReadString();  // add one new move
                        Console.WriteLine(str);
                        Console.WriteLine(winner.ToString());
                        Turn = true;
                    }
                    Moves.Add(str);
                }
            }

            catch (Exception ex)
            { // any exception can case gamed to end

                Console.WriteLine(ex.Message);
                winner = true;
            }
            finally
            {
                Console.WriteLine(Moves[Moves.Count-1]);
                // write last charachter played
                try
                {
                    PlayersList[i].sendData.Write(winner);
                    PlayersList[i].sendData.Write(str);
                }
                catch (Exception) { }

                try
                {
                    PlayersList[j].sendData.Write(winner);
                    PlayersList[j].sendData.Write(str);
                }
                catch { }

            }

            //    try
            //    {
            //        PlayersList[i].sendData.Write(winner);
            //        PlayersList[i].sendData.Write(Moves[Moves.Count-1]);
            //    }
            //    catch (Exception) { }

            //    try
            //    {
            //        PlayersList[j].sendData.Write(winner);
            //        PlayersList[j].sendData.Write(Moves[Moves.Count - 1]);
            //    }
            //    catch { }



            //try
            //{
            //    PlayersList[i].sendData.Write(Moves[Moves.Count - 1]);
            //}
            //catch (Exception) { }
            //try { PlayersList[j].sendData.Write(Moves[Moves.Count - 1]); }
            //catch { }

            EndConnection(PlayersList[i].sendData, PlayersList[i].readData, PlayersList[i].nStream);

            EndConnection(PlayersList[j].sendData, PlayersList[j].readData, PlayersList[j].nStream);

            Console.WriteLine("End");

        }
        public void StartGameOnePlayer(Client Player)
        {
            //start game
            try
            {
                Player.sendData.Write("start " +FirstChar);

                Swap();
                // response from player 
                string[] ar;
                int num;

                //client.Player = PlayerType.SinglePlayer;

                while (!Player.EndGame)
                {
                    str = Player.readData.ReadString();
                    ar = str.Split(' ');
                    if (Player.Moves.Count <= 9)
                    {
                        if (ar[0] == "play")
                        {
                            Player.Moves.Add(ar[1]);
                            num = GenerateRandom(Player);
                            Player.Moves.Add(num.ToString());
                            Player.sendData.Write(num.ToString());
                        }
                        else
                        {
                            Player.EndGame = true;
                            EndConnection(Player.sendData, Player.readData, Player.nStream);
                        }
                    }
                }
            }
            catch
            {
                EndConnection(Player.sendData, Player.readData, Player.nStream);
            }
        }
        public int GenerateRandom(Client client)
        {
            Random random = new Random();
            int x = 0;
            do
            {
                x = random.Next(1, 10);
            }
            while (client.Moves.Contains(x.ToString()));
            return x;
        }
        private void Swap()
        {
            string temp;

            temp = FirstChar;
            FirstChar = SecondChar;
            SecondChar = temp;
        }
        private void EndConnection(BinaryWriter sendData, BinaryReader readData, NetworkStream nStream)
        {
            sendData.Close();
            readData.Close();
            nStream.Close();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.EstablishConnection();
        }
    }

}
