//Arp -a sorgusu gönderiyor...
//Sorguları kaydediyor...
//Daha önce kaydettiği sorgu ile şimdikini karşılaştırıp farklı IP'leri buluyor.
//
//
//
//
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.IO;

namespace ArpMacIp
{
    public partial class Form1 : Form
    {
        
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int dstIp, int srcIp, byte[] mac, ref int macLen);

        public Form1()
        {
            InitializeComponent();
        }
        private static string GetARPResult()
        {
            Process p = null;
            string output = string.Empty;

            try
            {
                p = Process.Start(new ProcessStartInfo("arp", "-a")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                output = p.StandardOutput.ReadToEnd();

                p.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Arp a sorgusu gönderilemedi", ex);
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                }
            }

            return output;
        }

        private void button1_Click(object sender, EventArgs e)
        {    // IP adresleri arp -a sorgusunda oluşan ifadeden ayıklıyor.
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection sonuc = ip.Matches(GetARPResult());
            var ipler = new string[sonuc.Count];
            for (int i = 0; i < ipler.Length; i++){
                ipler[i] = sonuc[i].ToString();               
            }
            listBox2.Items.Clear();
            // MAC adresleri arp -a sorgusunda oluşan ifadeden ayıklıyor.
            foreach (string listeyeEkle in ipler)
              {
                string ifade = listeyeEkle;
                IPAddress tekil = IPAddress.Parse(ifade);
                var mac = new byte[6];
                var macLen = mac.Length;
                var duzenli = "xx-xx-xx-xx"; 
                if (SendARP((int)tekil.Address, 0, mac, ref macLen) == 0)
                             {
                    duzenli = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
                             }
                listBox2.Items.Add(duzenli);
           }
            listBox1.Items.Clear();
            foreach (string listeyeEkle in ipler)
            {
                listBox1.Items.Add(listeyeEkle);
            }
           // ilk arp -a sorgusu sonucu oluşan IP'leri kaydediyoruz ki ikinci sorgu yaparsak karşılaştıralım.
            string sPath = string.Format("ilk_sorgu.txt");
            System.IO.StreamWriter metin_belgesi = new System.IO.StreamWriter(sPath);
            foreach (var item in listBox1.Items)
            {
                metin_belgesi.WriteLine(item);
            }
            metin_belgesi.Close();
        }
        private void button2_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            //kaydetmek için Aslında üst taraftaki sorgudan alınabilirdi ama 
            //ben tekrar arp -a sorgusu göndererek tekrar ip ve mac adresleri ayıklayıp kaydetmeyi tercih ettim.
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection ayiklama_sonucu = ip.Matches(GetARPResult());
            var ipler = new string[ayiklama_sonucu.Count];
            for (int i = 0; i < ipler.Length; i++)
            {
                ipler[i] = ayiklama_sonucu[i].ToString();
            }
            listBox1.Items.Clear();
            foreach (string listeyeEkle in ipler)
            {
                listBox1.Items.Add(listeyeEkle);
            }
            foreach (string listeyeEkle in ipler)
            {
                string ifade = listeyeEkle;
                IPAddress tekil = IPAddress.Parse(ifade);
                var mac = new byte[6];
                var macLen = mac.Length;
                var duzenli = "xx-xx-xx-xx";
                if (SendARP((int)tekil.Address, 0, mac, ref macLen) == 0)
                {
                    duzenli = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
                }
                listBox2.Items.Add(duzenli);
            }
            // kaydedilen dosya adını yıl ay gün saat dakika ve snaiye olarak kaydettirdim ki ilerde LOG tutunca işe yarayabilir.
            string sPath = string.Format("kayit{0:-yyyy-MM-dd-hh-mm-ss}.txt", DateTime.Now);
            string sPath_mac = string.Format("ilk_mac.txt", DateTime.Now);  
            System.IO.StreamWriter Dosyam = new System.IO.StreamWriter(sPath);
            System.IO.StreamWriter Dosyam_mac = new System.IO.StreamWriter(sPath_mac);
            string sPathadi = string.Format("yol_adi.txt");           
            foreach (var item in listBox1.Items)
            {
                Dosyam.WriteLine(item);
            }
            Dosyam.Close();
            foreach (var item in listBox2.Items)
            {
                Dosyam_mac.WriteLine(item);
            }
            Dosyam_mac.Close();
            label2.Text = "KAYDEDİLDİ.! (adı ve konumu):";
            label3.Text= sPath;
            //yol adınıda yazdırıyorum ki programı kapatıp açtığında en son hangi tarihte kayıt oluşturulduğunu görmek işe yarayabilir.
            System.IO.StreamWriter Dosyamk = new System.IO.StreamWriter(sPathadi);
            Dosyamk.Write(sPath);
            Dosyamk.Close();    
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //şuanki ipleri listeye ekleyip son kayıt ile şuanki kayıt arasındaki farkı bulmak için kullanacağız.
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection result = ip.Matches(GetARPResult());
            var ipler = new string[result.Count];
            for (int i = 0; i < ipler.Length; i++)
            {
                ipler[i] = result[i].ToString();
            }
            listBox1.Items.Clear();
            foreach (string listeyeEkle in ipler)
            {
                listBox4.Items.Add(listeyeEkle);
            }
           
            // karsilastirmak icin kaydediyorum.
            string sPath = string.Format("simdiki.txt");
            System.IO.StreamWriter simdiki = new System.IO.StreamWriter(sPath);
            foreach (var item in listBox4.Items)
            {
                simdiki.WriteLine(item);
            }
            simdiki.Close();
            StreamReader yol_adi;
            string dosya_ici, yoladi;
            yol_adi = File.OpenText("yol_adi.txt");
            while ((dosya_ici = yol_adi.ReadLine()) != null)
            {
                label11.Text = dosya_ici.ToString();
            }
            yol_adi.Close();
            yoladi= label11.Text;
            //IPlerdeki farkı bulup ekrana (listbox5'e) yazdırıyoruz.
            string[] dosya1 = System.IO.File.ReadAllLines("simdiki.txt");
            string[] dosya2 = System.IO.File.ReadAllLines("ilk_sorgu.txt");
            IEnumerable<string> fark= dosya1.Except(dosya2);

            if (fark.Any()){
                foreach (string s in fark)
                {
                    listBox5.Items.Add(s);

                }
            }else{
               
                 listBox5.Items.Add("IP ler değişmedi.");
            }
         
       }          

        private void Form1_Load(object sender, EventArgs e)
        { //form açılır açılmaz en son kaydedilen dosyanın adını ekrana label olarak yazması işimize yarayabilir.
            StreamReader eski_dosya, yoladi,mac_listesi;
            eski_dosya = File.OpenText("ilk_sorgu.txt");
            mac_listesi = File.OpenText("ilk_mac.txt");
            string yazi, yazi2, yazi3;
            while ((yazi = mac_listesi.ReadLine()) != null)
            {
                listBox8.Items.Add(yazi.ToString());
            }
            mac_listesi.Close();
            while ((yazi2 = eski_dosya.ReadLine()) != null)
            {
                listBox3.Items.Add(yazi2.ToString());
            }
            eski_dosya.Close();
            yoladi = File.OpenText("yol_adi.txt");
            while ((yazi3 = yoladi.ReadLine()) != null)
            {
               label10.Text=yazi3.ToString();
            }
            yoladi.Close();     
              
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        
        // kısaca anlatayım: yukardaki işlemlee IP içindi bu aşağıdaki işlemler MAC için yapıyor. Yani MAC adresleri kontrol ediyor kaydediyor, karşılaştırıyor.
        private void button4_Click(object sender, EventArgs e)
        {
            //şuanki ipleri listeye ekleyip son kayıt ile şuanki kayıt arasındaki farkı bulmak için kullanacağız.
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection result = ip.Matches(GetARPResult());
            var ipler = new string[result.Count];
            for (int i = 0; i < ipler.Length; i++)
            {
                ipler[i] = result[i].ToString();
            }
            listBox7.Items.Clear();
            foreach (string listeyeEkle in ipler)
            {
                string ifade = listeyeEkle;
                IPAddress tekil = IPAddress.Parse(ifade);
                var mac = new byte[6];
                var macLen = mac.Length;
                var duzenli = "xx-xx-xx-xx";
                if (SendARP((int)tekil.Address, 0, mac, ref macLen) == 0)
                {
                    duzenli = string.Format("{0:x2}-{1:x2}-{2:x2}-{3:x2}-{4:x2}-{5:x2}", mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
                }
                listBox7.Items.Add(duzenli);
            }
            

            // karsilastirmak icin kaydediyorum.
            string sPath = string.Format("simdiki_mac.txt");
            System.IO.StreamWriter simdiki_mac = new System.IO.StreamWriter(sPath);
            foreach (var item in listBox7.Items)
            {
                simdiki_mac.WriteLine(item);
            }
            simdiki_mac.Close();
            StreamReader yol_adi;
            string dosya_ici, yoladi;
            yol_adi = File.OpenText("yol_adi.txt");
            while ((dosya_ici = yol_adi.ReadLine()) != null)
            {
                label11.Text = dosya_ici.ToString();
            }
            yol_adi.Close();
            yoladi = label11.Text;
            //IPlerdeki farkı bulup ekrana (listbox5'e) yazdırıyoruz.
            string[] dosya1 = System.IO.File.ReadAllLines("simdiki_mac.txt");
            string[] dosya2 = System.IO.File.ReadAllLines("ilk_mac.txt");
            IEnumerable<string> fark = dosya1.Except(dosya2);

            if (fark.Any())
            {
                foreach (string s in fark)
                {
                    listBox6.Items.Add(s);

                }
            }
            else
            {

                listBox6.Items.Add("Mac adresleri değişmedi.");
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Bu uygulama arp -a sorgusu gönderip, ip ve mac çıktıları verir. Eski tarihli sorgu ile karşılaştırma yapar. Yeni IP yada Yeni MAC adresi eklenmiş mi onun kontrolünü bu uygulama ile bakabiliriz.");
        }

       
    }
}
