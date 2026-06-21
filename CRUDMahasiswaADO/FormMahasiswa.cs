using ExcelDataReader;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services.Description;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class FormMahasiswa : Form
    {
        DAL dbLogic = new DAL();
        
        
        //private readonly SqlConnection conn;
        //private readonly string connectionString ="Data Source=KAIDEN\\BLAZE;Initial Catalog=DBAkademikADO;Integrated Security=True";

        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        public FormMahasiswa()
        {
            InitializeComponent();
            //conn = new SqlConnection(connectionString);
        }

        // ================== METHOD LOGGING (Sesuai Struktur Tabel LogError) ==================
        private void SimpanLog(string pesan)
        {
            dbLogic.InsertLog(pesan);
        }

        // ================== LOGIKA UTAMA (STORED PROCEDURE) ==================

        private void HitungTotal()
        {
            try
            {
                int total = (dbLogic.CountMhs().Equals(DBNull.Value))
                            ? 0 : dbLogic.CountMhs();
                lblTotal.Text = "Total Mahasiswa : " + total;
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                bindingSource.DataSource = dbLogic.GetMhs();
                dataGridView1.DataSource = bindingSource;

                // Supaya kolom Foto tampil sebagai gambar
                DataGridViewImageColumn fotoColumn =
                    (DataGridViewImageColumn)dataGridView1.Columns["Foto"];
                fotoColumn.ImageLayout = DataGridViewImageCellLayout.Stretch;

                HitungTotal();

                dataGridView1.Enabled = true;
                btnImpDB.Enabled = false;
                btnInsert_Click_2(null, null); // opsional, sesuaikan nama button insert Anda
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }
        private void ClearForm()
        {
            txtNIM.Enabled = true;
            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtKodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            fotoMhs.Image = null;
            txtNIM.Focus();
        }

        // ================== EVENT HANDLERS FORM ==================

        private void FormMahasiswa_Load_1(object sender, EventArgs e)
        {
            this.mahasiswaTableAdapter2.Fill(this.dBAkademikADODataSet.Mahasiswa);
            this.mahasiswaTableAdapter1.Fill(this.dBAkademikADODataSet11.Mahasiswa);

            cmbJK.Items.Clear();
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");
            cmbJK.SelectedIndex = -1; // tambahkan ini

            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            if (bindingNavigator1 != null)
            {
                bindingNavigator1.BindingSource = bindingSource;
            }

            LoadData();
        }

        // ====== TOMBOL INSERT (MENDUKUNG LOGAKTIVITAS & TRANSAKSI) ======
        private void btnInsert_Click_2(object sender, EventArgs e)
        {
            // Validasi input
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("NIM tidak boleh kosong!");
                return;
            }
            if (cmbJK.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih Jenis Kelamin terlebih dahulu!");
                return;
            }
            if (string.IsNullOrEmpty(txtKodeProdi.Text))
            {
                MessageBox.Show("Kode Prodi tidak boleh kosong!");
                return;
            }

            try
            {
                byte[] ConvertImageToBytes(System.Windows.Forms.PictureBox pb)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }

                byte[] imgBytes = fotoMhs.Image != null
                                  ? ConvertImageToBytes(fotoMhs)
                                  : null;

                dbLogic.InsertMhs(
                    txtNIM.Text.Trim(),
                    txtNama.Text.Trim(),
                    txtAlamat.Text.Trim(),
                    cmbJK.SelectedItem.ToString(), // pakai SelectedItem bukan Text
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text.Trim(),
                    imgBytes);

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // ================== ACTION BUTTONS LAINNYA ==================

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] ConvertImageToBytes(System.Windows.Forms.PictureBox pb)
                {
                    using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                    {
                        pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }

                byte[] imgBytes = fotoMhs.Image != null
                                  ? ConvertImageToBytes(fotoMhs)
                                  : null;

                dbLogic.UpdateMhs(
                    txtNIM.Text,
                    txtNama.Text,
                    txtAlamat.Text,
                    cmbJK.Text,
                    dtpTanggalLahir.Value.Date,
                    txtKodeProdi.Text,
                    imgBytes);

                MessageBox.Show("Data mahasiswa berhasil diubah");
                ClearForm();
                btnLoad_Click(null, null);
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dg = MessageBox.Show(
                    "Yakin ingin menghapus data?",
                    "Konfirmasi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (dg == DialogResult.Yes)
                {
                    dbLogic.DeleteMhs(txtNIM.Text);
                    MessageBox.Show("Data mahasiswa berhasil dihapus");
                    ClearForm();
                    btnLoad_Click(null, null);
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.resetData();
                MessageBox.Show("Data berhasil direset");
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        // ====== TOMBOL UJI COBA SQL INJECTION (MEMICU TRG_PREVENTMASSUPDATE & LOGKEAMANAN) ======
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                dbLogic.testInject(txtNIM.Text);
                LoadData();
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("safe"))
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error : Unsafe UPDATE operation not allowed");
                }
                else
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error :" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // Ganti pakai DAL
                using (SqlConnection connection =
                       new SqlConnection(dbLogic.GetConnectionString()))
                {
                    connection.Open();
                    MessageBox.Show("Koneksi berhasil ke " + connection.Database);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e) { LoadData(); }

        // ================== PLACEHOLDER CONTROLS ==================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) 
        {
            if (e.RowIndex >= 0)
            {
                DataRow row = ((DataRowView)bindingSource[e.RowIndex]).Row;

                txtNIM.Text = row[0].ToString();
                txtNama.Text = row[1].ToString();
                cmbJK.Text = row[2].ToString();
                dtpTanggalLahir.Value = Convert.ToDateTime(row[3]);
                txtAlamat.Text = row[4].ToString();
                txtKodeProdi.Text = row[6].ToString();

                if (row[5] != DBNull.Value)
                {
                    byte[] imgBytes = (byte[])row[5];
                    using (System.IO.MemoryStream ms =
                           new System.IO.MemoryStream(imgBytes))
                    {
                        fotoMhs.Image = System.Drawing.Image.FromStream(ms);
                        fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                }
                else
                {
                    fotoMhs.Image = null;
                }

                txtNIM.Enabled = false;
            }
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fotoMhs.Image = System.Drawing.Image.FromFile(ofd.FileName);
                fotoMhs.SizeMode = PictureBoxSizeMode.StretchImage;
            }

        }

        private void btnImpExcel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog =
           new OpenFileDialog { Filter = "Excel Workbook| *.xlsx" })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    using (var stream = System.IO.File.Open(
                           filePath, System.IO.FileMode.Open,
                           System.IO.FileAccess.Read))
                    {
                        using (var reader =
                               ExcelDataReader.ExcelReaderFactory.CreateReader(stream))
                        {
                            var result = reader.AsDataSet(
                                new ExcelDataReader.ExcelDataSetConfiguration()
                                {
                                    ConfigureDataTable = (_) =>
                                        new ExcelDataReader.ExcelDataTableConfiguration()
                                        {
                                            UseHeaderRow = true
                                        }
                                });

                            DataTable dt = result.Tables[0];
                            dataGridView1.DataSource = dt;
                            dataGridView1.Enabled = false;

                            btnImpDB.Enabled = true;
                            btnInsert_Click_2(null, null); // nonaktifkan tombol lain
                        }
                    }
                }
            }
        }

        private void btnImpDB_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dataGridView1.DataSource;

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diimport.");
                    return;
                }

                int sukses = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string nim = row["NIM"].ToString().Trim();
                    string nama = row["Nama"].ToString().Trim();
                    string jk = row["JenisKelamin"].ToString().Trim();
                    string alamat = row["Alamat"].ToString().Trim();
                    string kodeProdi = row["NamaProdi"].ToString().Trim();
                    string fotoPath = dt.Columns.Contains("FotoPath")
                                       ? row["FotoPath"].ToString().Trim()
                                       : string.Empty;

                    if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama))
                        continue;

                    DateTime tglLahir;
                    if (!DateTime.TryParse(row["TanggalLahir"].ToString(), out tglLahir))
                        continue;

                    byte[] ConvertImageFromPath(string path)
                    {
                        if (string.IsNullOrWhiteSpace(path)) return null;
                        if (!System.IO.File.Exists(path)) return null;
                        return System.IO.File.ReadAllBytes(path);
                    }

                    byte[] fotoBytes = ConvertImageFromPath(fotoPath);

                    dbLogic.InsertMhs(nim, nama, alamat, jk,
                                      tglLahir, kodeProdi, fotoBytes);
                    sukses++;
                }

                MessageBox.Show("Data mahasiswa berhasil ditambahkan");
                ClearForm();
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog("Rollback Insert : " + ex.Message);
                MessageBox.Show("SQL Error :" + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog("General Error :" + ex.Message);
                MessageBox.Show("General Error :" + ex.Message);
            }
        }

        private void BtnRekap_Click(object sender, EventArgs e)
        {
            RekapMahasiswa fm2 = new RekapMahasiswa();
            fm2.Show();
            this.Hide();
        }

        private void txtKodeProdi_TextChanged(object sender, EventArgs e) { }
        private void bindingNavigatorPositionItem_Click(object sender, EventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }
        private void btnUpdate_Click_2(object sender, EventArgs e) { button1_Click(sender, e); }

    }
}