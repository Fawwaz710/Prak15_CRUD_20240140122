using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace CRUDMahasiswaADO
{
    public partial class Report : Form
    {
        // Gunakan readonly atau private biasa untuk connection string
        private static string connectionString = "Data Source=KAIDEN\\BLAZE;Initial Catalog=DBAkademikADO;User ID=sa;Password=towinnadzul09122005";

        private SqlConnection conn = new SqlConnection(connectionString);
        private SqlDataAdapter da;
        private DataTable dtMahasiswa;

        // PERBAIKAN DI SINI: Ubah nama objek variabelnya menjadi 'rptMahasiswa' (huruf kecil/berbeda)
        // agar tidak bentrok dengan nama Class 'LaporanMahasiswa'
        private LaporanMahasiswa rptMahasiswa = new LaporanMahasiswa();

        string prodi { get; set; }
        DateTime tglmasuk { get; set; }

        public Report(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();

            prodi = Prodi;
            tglmasuk = TglMasuk;

            try
            {
                DataTable dtMahasiswa = dbLogic.getDataRekap(prodi, tglmasuk);

                listMahasiswa.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = listMahasiswa;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void crystalReportViewer1_Load(object sender, EventArgs e)
        {
            // Bisa dikosongkan jika tidak dipakai
        }
    }
}