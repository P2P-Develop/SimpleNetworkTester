using System;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Input;

namespace NetworkTester
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Basic Event Handlers

        private void InputNumber(object sender, TextCompositionEventArgs e)
        {
            bool cut = false;
            string check = "0123456789\b";

            if (!check.Contains(e.Text))
            {
                cut = true;
            }

            e.Handled = cut;
        }

        #endregion

        #region Ping Methods

        private void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            string _pingCount;
            string _timeOutSeconds;

            if (tbIPAddress.Text.Length == 0)
            {
                tbPingError.Text = "IPアドレスが入力されていません。";
                return;
            }

            if (tbTimeoutSeconds.Text.Length != 0)
            {
                _timeOutSeconds = tbTimeoutSeconds.Text;
            }
            else
            {
                _timeOutSeconds = "1";
            }
            if (tbPingCount.Text.Length != 0)
            {
                _pingCount = tbPingCount.Text;
            }
            else
            {
                _pingCount = "5";
            }
            StartPing(tbIPAddress.Text, _timeOutSeconds, _pingCount);
        }

        /// <summary>
        /// 指定したIPアドレスにPingを送信します。
        /// </summary>
        /// <param name="address">Ping送信対象のアドレス。</param>
        /// <param name="timeout">次のPingへのタイムアウト。</param>
        /// <param name="countofping">Pingを送信する回数。</param>
        private void StartPing(string address, string timeout, string countofping)
        {
            tbConsole.AppendText("\nStarting Ping... | Address: " + address + ", Count: " + countofping + ", TimeOut: " + timeout);
            try
            {
                if (int.TryParse(timeout, out int timeOutSeconds))
                {
                    if (int.TryParse(countofping, out int pingCount))
                    {
                        Ping ping = new Ping();
                        if (address == "localhost")
                        {
                            address = "127.0.0.1";
                        }

                        for (int i = 0; i < pingCount; i++)
                        {
                            PingReply reply = ping.Send(address);
                            if (reply.Status == IPStatus.Success)
                            {
                                tbConsole.AppendText("\nReply from " + reply.Address.ToString() + ": bytes=" + reply.Buffer.Length.ToString() + " time=" + reply.RoundtripTime.ToString() + "ms TTL=" + reply.Options.Ttl.ToString());
                            }
                            else
                            {
                                tbConsole.AppendText("\n" + reply.Status.ToString());
                            }

                            // ping送信の間隔を取る
                            if (i < pingCount)
                            {
                                System.Threading.Thread.Sleep(timeOutSeconds * 1000);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                tbConsole.AppendText("\nPing Failed");
                tbConsole.AppendText("\n" + ex.Message);
                tbConsole.AppendText("\nStackTrace: \n" + ex.StackTrace);
            }
        }

        #endregion

        #region Port Check Methods

        private void CbIsSSLPort_Checked(object sender, RoutedEventArgs e)
        {
            cbPort.Visibility = Visibility.Collapsed;
            cbSSLPort.Visibility = Visibility.Visible;
        }

        private void CbIsSSLPort_Unchecked(object sender, RoutedEventArgs e)
        {
            cbPort.Visibility = Visibility.Visible;
            cbSSLPort.Visibility = Visibility.Collapsed;
        }

        private void BtnPortCheck_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(tbPort.Text, out int port))
            {
                if (PortCheck(tbIPAddress.Text, port, ssl: (bool)cbIsSSLPort.IsChecked))
                {
                    tbConsole.AppendText("\nPortCheck Successfly Completed.");
                    tbPortError.Visibility = Visibility.Collapsed;
                    tbPortSuccess.Visibility = Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// 指定したIPアドレスのポートが開放出来ているかを<see cref="TcpClient"/>で確認します。未完成です。
        /// </summary>
        /// <param name="address">ポートチェックの対象アドレス。</param>
        /// <param name="port">ポートチェック対象のポート。</param>
        /// <param name="ssl">SSLポートのチェックを行うかどうか。</param>
        /// <returns>ポートチェックの結果。</returns>
        private bool PortCheck(string address, int port, bool ssl)
        {
            if (address == "localhost")
            {
                address = "127.0.0.1";
            }

            if (ssl)
            {
                // 未完成
                return false;
            }
            else
            {
                try
                {
                    using (TcpClient tcp = new TcpClient(address, port))
                    {
                        tbConsole.AppendText("\nStarting PortCheck... | Address: " + address + ", Port: " + port);
                        tcp.Connect(address, port);
                        tcp.Close();
                        return tcp.Connected;
                    }
                }catch (SocketException ex)
                {
                    tbPortSuccess.Visibility = Visibility.Collapsed;
                    tbPortError.Visibility = Visibility.Visible;
                    tbPortError.Text = "接続中にエラーが発生しました。コンソールを確認してください。";
                    tbConsole.AppendText("\nPortCheck Failed. | Errorcode: " + ex.SocketErrorCode + "\n" + ex.Message + "\nStackTrace: \n" + ex.StackTrace);
                    return false;
                }
            }
        }

        #endregion
    }
}
