using BomberMan.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace BomberMan
{
    public partial class frmGioco : Form
    {
        public int portaUdpDefault = 5000;
        private const int portaUdpCatch = 5001;
        private int cont = 0;
        public int portaLocaleUdp;
        public int portaRemotaUdp;
        public string strIpRemoto;       // l'indirizzo IP del destinatario
        string username;
        public Socket udpSocket;     // socket per ricevere e trasmettere
        public EndPoint endP;          // l'endpoint dell'altro capo (sia in ricezione che in spedizione) da usare con le routine asincrone di C#
        public byte[] abytRx = new byte[1024];  // il buffer di ricezione
        public byte[] abytTx = new byte[1024];  // il buffer di spedizione
        public int valSopra;
        public int valSotto;
        public int valDestra;
        public int valSinistra;
        public int posizioneBomba;
        public System.Windows.Forms.Timer timerBomba = new System.Windows.Forms.Timer();
        public int posizionePersonaggio1;
        public int posizionePersonaggio2;
        PictureBox[] esplosioneSopra = new PictureBox[12];
        PictureBox[] esplosioneSotto = new PictureBox[12];
        PictureBox[] esplosioneDestra = new PictureBox[12];
        PictureBox[] esplosioneSinistra = new PictureBox[12];
        PictureBox imgBomba;
        int contErrore;
        const string nomeFile = "file.xml";
        string strFile;
        public char[,] mappa = new char[11, 11]
        {
            { 'X','0','0','0','0','0','0','0','0','0','0'},
            { '0','#','0','#','0','#','0','#','0','#','0'},
            { '0','0','0','0','0','0','0','0','0','0','0'},
            { '0','#','0','#','0','#','0','#','0','#','0'},
            { '0','0','0','0','0','0','0','0','0','0','0'},
            { '0','#','0','#','0','#','0','#','0','#','0'},
            { '0','0','0','0','0','0','0','0','0','0','0'},
            { '0','#','0','#','0','#','0','#','0','#','0'},
            { '0','0','0','0','0','0','0','0','0','0','0'},
            { '0','#','0','#','0','#','0','#','0','#','0'},
            { '0','0','0','0','0','0','0','0','0','0','Y'},
        };
        
        public frmGioco()
        {
            InitializeComponent();
            panelGioco.Visible = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "BOMBERMAN";

            strIpRemoto = IPAddress.Loopback.ToString();
            portaRemotaUdp = portaUdpDefault;
            portaLocaleUdp = portaUdpDefault;

            string localIP;
            IPEndPoint endPoint;
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            using (socket)
            {
                socket.Connect("1.0.0.0", 30000);
                endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
                indirizzoIp.Text = localIP;
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            strFile = Application.StartupPath + "\\" + nomeFile;
            dati d = new dati();
            d = DeserializzazioneDatiXml();

            textPortalocale.Text = d.portaLocale;
            textPortaRemota.Text = d.portaRemota;
            textIpDestinatario.Text = d.indirizzoIP;
            textUsername.Text = d.username;
        }

        /*-------------------------------------------GIOCO----------------------------------------------------------*/

        public void eliminaEspolosione(PictureBox[] pic)
        {
            try
            {
                for (int i = 0; i < pic.Length; i++)
                {
                    pic[i].Visible = false;
                    this.Controls.Remove(pic[i]);
                    pic[i].SendToBack();
                    pic[i].SendToBack();
                }
            }
            catch
            {

            }
        }

        public Boolean moveInMappa(int mode, int posizione)
        {
            int val = posizione;
            int[] valori = new int[] {11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 121 ,132};

            //Su
            if (mode == 0)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (val < 11)
                    {
                        if (val < valori[i])
                        {
                            if (mappa[0, val] != '#')
                                return true;
                        }
                    }
                    else
                    {
                        if (val >= valori[i] && val < valori[i + 1])
                        {
                            val -= valori[i];
                         
                            if (val % 2 == 0)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }

            //Giù
            if (mode == 1)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (val < 11)
                    {
                        if (val < valori[i])
                        {
                            if (mappa[1, val] != '#')
                                return true;
                        }
                    }
                    else
                    {
                        if (val >= valori[i] && val < valori[i + 1])
                        {
                            val -= valori[i];
                          
                            if (val % 2 == 0)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }
            //Destra
            if (mode == 2)
            {
                for (int i = 0; i < 11; i++)
                {
                    if (val < 11)
                    {
                        if (val < valori[i])
                        {
                            if (val < 9)
                            {

                                if (mappa[0, val + 1] != '#')
                                    return true;
                            }
                            else
                            {
                                if (mappa[0, val] != '#')
                                    return true;
                            }
                        }
                    }
                    else
                    {
                        if (val >= valori[i] && val < valori[i + 1])
                        {
                            val -= valori[i];
                            
                            if (i % 2 == 1)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }
            //Sinistra
            if (mode == 3)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (val < 11)
                    {
                        if (val < valori[i])
                        {
                            if (mappa[0, val - 1] != '#')
                                return true;
                        }
                    }
                    else
                    {
                        if (val >= valori[i] && val < valori[i + 1])
                        {
                            val -= valori[i];
                           
                            if (i % 2 == 1)
                                return true;
                            else
                                return false;
                        }
                    }
                }
            }
            return false;
        }

        public void direzioneBomba()
        {
            int val = posizioneBomba;
            int[] valori = new int[] { 11, 22, 33, 44, 55, 66, 77, 88, 99, 110, 121, 132 };

            for (int i = 0; i < 12; i++)
            {
                if (posizioneBomba < 11)
                {
                    valSopra = 0;
                    valDestra = 11;
                    valSinistra = 11;
                    if (val % 2 == 0)
                        valSotto = 11;
                    if (val % 2 == 1)
                        valSotto = 0;
                }
                else
                {
                    if (val >= valori[i] && val < valori[i + 1])
                    {
                        val -= valori[i];

                        if (val % 2 == 0 && i % 2 == 1)
                        {
                            valSopra = 11;
                            valSotto = 11;
                            valDestra = 11;
                            valSinistra = 11;
                        }
                        if (val % 2 == 1 && i % 2 == 1)
                        {
                            valSopra = 0;
                            valSotto = 0;
                            valDestra = 11;
                            valSinistra = 11;
                        }
                        if (val % 2 == 0 && i % 2 == 0)
                        {
                            valSopra = 11;
                            valSotto = 11;
                            valDestra = 0;
                            valSinistra = 0;
                        }

                        if (val % 2 == 1 && i % 2 == 0)
                        {
                            valSopra = 0;
                            valSotto = 0;
                            valDestra = 0;
                            valSinistra = 0;
                        }
                    }
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            ///Up
            if (e.KeyCode == Keys.Up && imgPersonaggio.Location.Y > 16 && this.moveInMappa(0, posizionePersonaggio1))
            {
                imgPersonaggio.Location = new System.Drawing.Point(imgPersonaggio.Location.X, imgPersonaggio.Location.Y - 38);
                posizionePersonaggio1 -= 11;
                Send("U" + imgPersonaggio.Location.X/40 + imgPersonaggio.Location.Y / 38);
            }

            ///Down
            if (e.KeyCode == Keys.Down && imgPersonaggio.Location.Y < 365 && this.moveInMappa(1, posizionePersonaggio1))
            {
                imgPersonaggio.Location = new System.Drawing.Point(imgPersonaggio.Location.X, imgPersonaggio.Location.Y + 38);
                posizionePersonaggio1 += 11;
                Send("D" + imgPersonaggio.Location.X / 40 + imgPersonaggio.Location.Y / 38);
            }
            ///Right
            if (e.KeyCode == Keys.Right && imgPersonaggio.Location.X < 400 && this.moveInMappa(2, posizionePersonaggio1))
            {
                imgPersonaggio.Location = new System.Drawing.Point(imgPersonaggio.Location.X + 40, imgPersonaggio.Location.Y);
                posizionePersonaggio1 += 1;
                Send("R" + imgPersonaggio.Location.X / 40 + imgPersonaggio.Location.Y / 38);
            }

            ///Left
            if (e.KeyCode == Keys.Left && imgPersonaggio.Location.X > 5 && this.moveInMappa(3, posizionePersonaggio1))
            {
                imgPersonaggio.Location = new System.Drawing.Point(imgPersonaggio.Location.X - 40, imgPersonaggio.Location.Y);
                posizionePersonaggio1 -= 1;
                Send("L" + imgPersonaggio.Location.X / 40 + imgPersonaggio.Location.Y / 38);
            }

            if (e.KeyCode == Keys.Space && cont == 0)
            {
                posizioneBomba = posizionePersonaggio1;
                mettiBomba(0);
                cont = 1;
                Send("B" + imgBomba.Location.X / 40 + imgBomba.Location.Y / 38);

                timerBomba.Interval = 3000;
                timerBomba.Start();
                timerBomba.Tick += new EventHandler(timer_Tick);

            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            imgBomba.Visible = false;
            this.direzioneBomba();
            int valEliminato1 = -1;
            int valEliminato2 = 0;
           
            if (valSopra == 11)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (imgBomba.Location.Y - i * 38 > 4)
                    {
                        esplosioneSopra[i] = new PictureBox();
                        esplosioneSopra[i].Image = Resources.fiamme;
                        esplosioneSopra[i].Location = new System.Drawing.Point(imgBomba.Location.X, imgBomba.Location.Y - i * 38);
                        esplosioneSopra[i].Size = new System.Drawing.Size(25, 33);
                        esplosioneSopra[i].Visible = true;
                        panelGioco.Controls.Add(esplosioneSopra[i]);
                        esplosioneSopra[i].BringToFront();
                       
                        if (imgPersonaggio.Location.X == imgBomba.Location.X && imgPersonaggio.Location.Y == imgBomba.Location.Y - i * 38)
                        {
                            valEliminato1 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                        if (imgPersonaggio2.Location.X == imgBomba.Location.X && imgPersonaggio2.Location.Y == imgBomba.Location.Y - i * 38)
                        {
                            valEliminato2 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                    }
                }
            }
            if (valSotto == 11)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (imgBomba.Location.Y + i * 38 < 400)
                    {
                        esplosioneSotto[i] = new PictureBox();
                        esplosioneSotto[i].Image = Resources.fiamme;
                        esplosioneSotto[i].Location = new System.Drawing.Point(imgBomba.Location.X, imgBomba.Location.Y + i * 38);
                        esplosioneSotto[i].Size = new System.Drawing.Size(25, 33);
                        panelGioco.Controls.Add(esplosioneSotto[i]);
                        esplosioneSotto[i].BringToFront();
                        esplosioneSotto[i].Visible = true;

                        if ((imgPersonaggio.Location.X == imgBomba.Location.X && imgPersonaggio.Location.Y == imgBomba.Location.Y + i * 38))
                        {
                            valEliminato1 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                        if (imgPersonaggio2.Location.X == imgBomba.Location.X && imgPersonaggio2.Location.Y == imgBomba.Location.Y + i * 38)
                        {
                            valEliminato2 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                    }
                }
            }
            if (valDestra == 11)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (imgBomba.Location.X + i * 38 < 465)
                    {
                        esplosioneDestra[i] = new PictureBox();
                        esplosioneDestra[i].Image = Resources.fiamme;
                        esplosioneDestra[i].Location = new System.Drawing.Point(imgBomba.Location.X + i * 40, imgBomba.Location.Y);
                        esplosioneDestra[i].Size = new System.Drawing.Size(25, 33);
                        panelGioco.Controls.Add(esplosioneDestra[i]);
                        esplosioneDestra[i].BringToFront();
                        esplosioneDestra[i].Visible = true;

                        if ((imgPersonaggio.Location.X == imgBomba.Location.X + i * 40 && imgPersonaggio.Location.Y == imgBomba.Location.Y))
                        {
                            valEliminato1 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                        if (imgPersonaggio2.Location.X == imgBomba.Location.X + i * 40 && imgPersonaggio2.Location.Y == imgBomba.Location.Y)
                        {
                            valEliminato2 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                    }
                }
            }
            if (valSinistra == 11)
            {
                for (int i = 0; i < 12; i++)
                {
                    if (imgBomba.Location.X - i * 38 > 13)
                    {
                        esplosioneSinistra[i] = new PictureBox();
                        esplosioneSinistra[i].Image = Resources.fiamme;
                        esplosioneSinistra[i].Location = new System.Drawing.Point(imgBomba.Location.X - i * 40, imgBomba.Location.Y);
                        esplosioneSinistra[i].Size = new System.Drawing.Size(25, 33);
                        panelGioco.Controls.Add(esplosioneSinistra[i]);
                        esplosioneSinistra[i].BringToFront();
                        esplosioneSinistra[i].Visible = true;

                        if ((imgPersonaggio.Location.X == imgBomba.Location.X - i * 40) && (imgPersonaggio.Location.Y == imgBomba.Location.Y))
                        {
                            valEliminato1 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                        if ((imgPersonaggio2.Location.X == imgBomba.Location.X - i * 40) && (imgPersonaggio2.Location.Y == imgBomba.Location.Y))
                        {
                            valEliminato2 = 1;
                            panelGioco.Visible = false;
                            setVisibleComponents(true);
                        }
                    }
                }
            }

            wait(1200);
            eliminaEspolosione(esplosioneDestra);
            eliminaEspolosione(esplosioneSinistra);
            eliminaEspolosione(esplosioneSopra);
            eliminaEspolosione(esplosioneSotto);
            timerBomba.Stop();

            if (valEliminato1 == 1 && valEliminato2 == 0)
                Send("F2");
            if (valEliminato2 == 1 && valEliminato1 == 0)
                Send("F1");
            if(valEliminato2 == 1 && valEliminato1 == 1)
                 Send("F0");
            
            cont = 0;
            valSopra = 0;
            valSotto = 0;
            valDestra = 0;
            valSinistra = 0;
        }

        public void wait(int millisecondi)
        {
            System.Windows.Forms.Timer timerEsplosione = new System.Windows.Forms.Timer();
            if (millisecondi == 0 || millisecondi < 0)
                return;
            
            timerEsplosione.Interval = millisecondi;
            timerEsplosione.Enabled = true;
            timerEsplosione.Start();
            timerEsplosione.Tick += (s, e) =>
            {
                timerEsplosione.Enabled = false;
                timerEsplosione.Stop();
            };
            while (timerEsplosione.Enabled)
            {
                Application.DoEvents();
            }
        }

        public void setVisibleComponents(Boolean val)
        {
            buttonConnetti.Enabled = val;
            btnBind.Enabled = val;
            textPortalocale.Enabled = val;
            textPortaRemota.Enabled = val;
            textIpDestinatario.Enabled = val;
            textUsername.Enabled = val;
            lst.Enabled = val;
        }

        public void muoviSu()
        {
             imgPersonaggio2.Location = new System.Drawing.Point(imgPersonaggio2.Location.X, imgPersonaggio2.Location.Y - 38);
             posizionePersonaggio2 -= 11;
        }

        public void muoviGiu()
        {
             imgPersonaggio2.Location = new System.Drawing.Point(imgPersonaggio2.Location.X, imgPersonaggio2.Location.Y + 38);
             posizionePersonaggio2 += 11;
        }

        public void muoviDestra()
        {
             imgPersonaggio2.Location = new System.Drawing.Point(imgPersonaggio2.Location.X + 40, imgPersonaggio2.Location.Y);
             posizionePersonaggio2 += 1; 
        }

        public void muoviSinistra()
        {
             imgPersonaggio2.Location = new System.Drawing.Point(imgPersonaggio2.Location.X - 40, imgPersonaggio2.Location.Y);
             posizionePersonaggio2 -= 1;
        }

        public void mettiBomba(int personaggio)
        {
            imgBomba = new PictureBox();
            imgBomba.Visible = true;
            panelGioco.Controls.Add(imgBomba);
            imgBomba.BringToFront();
            imgBomba.Image = Resources.Bombe2;
            imgBomba.Size = new System.Drawing.Size(25, 33);
            imgBomba.BackColor = Color.Transparent;
            
            if (personaggio == 0)
                imgBomba.Location = new System.Drawing.Point(imgPersonaggio.Location.X, imgPersonaggio.Location.Y);
            else
                imgBomba.Location = new System.Drawing.Point(imgPersonaggio2.Location.X, imgPersonaggio2.Location.Y);
        }

        /*-------------------------------------------LAN----------------------------------------------------------*/

        public void Bind()
        {
            try
            {
                portaLocaleUdp = Convert.ToInt16(textPortalocale.Text);
                portaRemotaUdp = Convert.ToInt16(textPortaRemota.Text);
                strIpRemoto = textIpDestinatario.Text;
                IPEndPoint ipEP;
                // Creazione socket Udp
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                // L'endpoint locale per la ricezione
                ipEP = new IPEndPoint(IPAddress.Any, portaLocaleUdp);
                // Associazione degli indirizzi al socket (per la ricezione): IP locale e Porta Locale
                udpSocket.Bind(ipEP);
                endP = (EndPoint)ipEP;
                // Impostazione della ricezione asincrona sul socket
                udpSocket.BeginReceiveFrom(abytRx, 0, abytRx.Length, SocketFlags.None, ref endP, new AsyncCallback(OnReceive), endP);
                lst.Items.Insert(0, "Pronto a ricevere sulla porta locale UDP " + portaLocaleUdp.ToString());
            }
            catch (SocketException e)
            {
                portaLocaleUdp = portaUdpCatch;
                portaRemotaUdp = portaUdpDefault;

                IPEndPoint ipEP;
                // Creazione socket Udp
                udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                // L'endpoint locale per la ricezione
                ipEP = new IPEndPoint(IPAddress.Any, portaLocaleUdp);
                // Associazione degli indirizzi al socket (per la ricezione): IP locale e Porta Locale
                udpSocket.Bind(ipEP);
                endP = (EndPoint)ipEP;
                // Impostazione della ricezione asincrona sul socket
                udpSocket.BeginReceiveFrom(abytRx, 0, abytRx.Length, SocketFlags.None, ref endP, new AsyncCallback(OnReceive), endP);
                textPortalocale.Text = portaLocaleUdp.ToString();
                textPortaRemota.Text = portaRemotaUdp.ToString();
                lst.Items.Insert(0, "Pronto a ricevere sulla porta locale UDP " + portaLocaleUdp.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Errore durante il Bind(): " + ex.Message);
                contErrore = 1;
            }
        }

        private void Send(string azione)
        {
            // Considero l'indirizzo Ip selezionato
            if (strIpRemoto != "")
            {
                string strMessage;
                IPEndPoint ipEP;
                IPAddress ipAddress;
                username = textUsername.Text;
                
                try
                {
                    // Ecco l'IPaddress dalla stringa con l'indirizzo IP
                    strIpRemoto = textIpDestinatario.Text;
                    ipAddress = IPAddress.Parse(strIpRemoto);
                
                    // L'endpoint remoto a cui spedire
                    ipEP = new IPEndPoint(ipAddress, portaRemotaUdp);
                    endP = (EndPoint)ipEP;
                    try
                    { 
                        strMessage = azione;
                        abytTx = Encoding.UTF8.GetBytes(strMessage);
                        // Spedizione asincrona del buffer di byte
                        udpSocket.BeginSendTo(abytTx, 0, strMessage.Length, SocketFlags.None, endP, new AsyncCallback(OnSend), null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Send(): eccezione Exception\n" + ex.Message);
                    }
                }
                catch
                {
                    MessageBox.Show("Errore nel Send() : Formato dell'indirizzo IP errato");
                }
            }
            else
            {
                lst.Items.Insert(0, "Indirizzo Ip destinazione mancante");
            }
        }

        private delegate void del_OnReceive(IAsyncResult ar);
        private void OnReceive(IAsyncResult ar)
        {
            if (InvokeRequired)  // Per gestire il crossthread (questa routine è chiamata da un altro thread)
            {
                BeginInvoke(new del_OnReceive(OnReceive), ar);
                return;
            }

            try
            {
                string strReceived;
                int idx;
                IPEndPoint ipEPRx;
                username = textUsername.Text;

                if (udpSocket == null)
                {
                    lst.Items.Insert(0, "Socket nullo");
                    return;
                }

                ipEPRx = new IPEndPoint(IPAddress.Any, 0);
                endP = (EndPoint)ipEPRx;
                // Ecco la fine della ricezione. Ora i dati ricevuti sono nel buffer globale
                udpSocket.EndReceiveFrom(ar, ref endP);

                // Recupero Ip e Porta dell'host remoto
                string[] astr = endP.ToString().Split(':');
                // Ecco il messaggio ricevuto. 
                strReceived = Encoding.UTF8.GetString(abytRx);  // trasformo in stringa i dati ricevuti

                // Prendo solo i caratteri che precedono il carattere nullo (il tipo string non è come l'array di char del C
                idx = strReceived.IndexOf((char)0);
                if (idx > -1) strReceived = strReceived.Substring(0, idx);

                if (strReceived.StartsWith("N") && !(strReceived.StartsWith("NOK")))
                {
                    DialogResult risultato = MessageBox.Show("Accetti la richiesta di giocare di " + strReceived.Substring(1),
                        "BOMBERMAN", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (risultato == DialogResult.Yes)
                    {
                        Send("OK" + username);
                        this.Text = "BOMBERMAN , Giocatore 1";
                        imgPersonaggio.Location = new System.Drawing.Point(0, 4);
                        imgPersonaggio2.Location = new System.Drawing.Point(400, 384);
                        posizionePersonaggio1 = 0;
                        posizionePersonaggio2 = 121;
                        panelGioco.Visible = true;
                        setVisibleComponents(false);
                    }
                    else
                    {
                        Send("NOK" + username);
                        setVisibleComponents(true);
                    }
                }

                if (strReceived.StartsWith("OK"))
                {
                    this.Text = "BOMBERMAN , Giocatore 2";
                    imgPersonaggio.Location = new System.Drawing.Point(0, 4);
                    imgPersonaggio2.Location = new System.Drawing.Point(400, 384);

                    imgPersonaggio.Image = Resources.personaggio4;
                    imgPersonaggio2.Image = Resources.personaggio3;
                    posizionePersonaggio1 = 0;
                    posizionePersonaggio2 = 121;
                    panelGioco.Visible = true;
                    setVisibleComponents(false);
                }
                
                if(strReceived.StartsWith("NOK"))
                    setVisibleComponents(true);

                if (strReceived.StartsWith("U"))
                    muoviGiu();

                if (strReceived.StartsWith("D"))
                    muoviSu();

                if (strReceived.StartsWith("R"))
                    muoviSinistra();

                if (strReceived.StartsWith("L"))
                    muoviDestra();

                if (strReceived.StartsWith("B"))
                {
                    mettiBomba(1);
                    timerBomba.Interval = 3000;
                    timerBomba.Start();
                    timerBomba.Tick += new EventHandler(timer_Tick);   
                }

                if (strReceived == "F0")
                {
                    MessageBox.Show("LA PARTITA E' FINITA IN PARITA'!!!");
                    panelGioco.Visible = false;
                    setVisibleComponents(true);
                }
                if (strReceived == "F1")
                {
                    MessageBox.Show("HAI PERSO LA PARTITA " + username + " !!!");
                    panelGioco.Visible = false;
                    setVisibleComponents(true);
                }
                if (strReceived == "F2")
                {
                    MessageBox.Show("HAI VINTO LA PARTITA" + username + "!!!");
                    panelGioco.Visible = false;
                    setVisibleComponents(true);
                }

                lst.Items.Insert(0, "<IP Remote: " + astr[0] + ", Remote Port: " + astr[1] + ">" + strReceived);  // Sul listbox

                // Reinizializzo il buffer con zeri, per evitare che la prossima ricezione sovrapponga la precedente
                abytRx = new byte[abytRx.Length];
                // Riassocio la routine di ricezione
                udpSocket.BeginReceiveFrom(abytRx, 0, abytRx.Length, SocketFlags.None, ref endP, new AsyncCallback(OnReceive), endP);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show("OnReceive(): Eccezione ObjectDisposedException\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("OnReceive(): Eccezione Exception\n" + ex.Message);
                if (udpSocket != null)
                {
                    udpSocket.Shutdown(SocketShutdown.Both);
                    udpSocket.Close();
                    udpSocket = null;
                    btnBind.Enabled = true;
                }
            }
        }

        private delegate void del_OnSend(IAsyncResult ar);
        private void OnSend(IAsyncResult ar)
        {
            if (InvokeRequired)  // Per gestire il crossthread (questa routine è chiamata da un altro thread)
            {
                BeginInvoke(new del_OnSend(OnSend), ar);
                return;
            }

            try
            {
                udpSocket.EndSend(ar);
            }
            catch (ObjectDisposedException ex)
            {
                MessageBox.Show("OnSend(): Eccezione ObjectDisposedException\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("OnSend(): Eccezione Exception\n" + ex.Message);
            }
        }

        private void btnBind_Click(object sender, EventArgs e)
        {
            Bind();
            if (contErrore == 0)
                btnBind.Enabled = false;
            else
                btnBind.Enabled = true;

            dati d = new dati();
            d.portaLocale = textPortalocale.Text;
            d.portaRemota = textPortaRemota.Text;
            d.indirizzoIP = textIpDestinatario.Text;
            d.username = textUsername.Text;

            SerializzazioneDatiXml(d);

            contErrore = 0;
        }

        private void buttonConnetti_Click(object sender, EventArgs e)
        {
            username = textUsername.Text;
            Send("N" + username);
        }

        /*-------------------------------------------Dati Persistenti----------------------------------------------------------*/

        public void SerializzazioneDatiXml(dati valoriDati)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(dati));
            TextWriter writer = new StreamWriter(strFile);

            // Salvataggio senza attributo di schema namespace nel file xml
            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();
            xmlNameSpace.Add("", "");
            serializer.Serialize(writer, valoriDati, xmlNameSpace);
            writer.Close();
        }

        public dati DeserializzazioneDatiXml()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(dati));
            TextReader reader = new StreamReader(strFile);
            object obj = deserializer.Deserialize(reader);
            dati d = (dati)obj;
            reader.Close();
            return d;
        }
    }
}

