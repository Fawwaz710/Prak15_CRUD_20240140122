using System;
using System.Data;
using System.Data.SqlClient;
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
            conn = new SqlConnection(connectionString);
        }

        // ================== METHOD LOGGING (Sesuai Struktur Tabel LogError) ==================
        private void SimpanLog(string pesan)
        {
            using (SqlConnection connectionLogError = new SqlConnection(connectionString))
            {
                // Disesuaikan dengan kolom tabel LogError: waktu, pesan_error
                string query = "INSERT INTO LogError (waktu, pesan_error) VALUES (GETDATE(), @pesan)";
                using (SqlCommand cmd = new SqlCommand(query, connectionLogError))
                {
                    cmd.Parameters.AddWithValue("@pesan", pesan);
                    connectionLogError.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ================== LOGIKA UTAMA (STORED PROCEDURE) ==================

        private void HitungTotal()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        connection.Open();
                        cmd.ExecuteNonQuery();

                        if (lblTotal != null)
                        {
                            lblTotal.Text = "Total Mahasiswa: " + outputParam.Value.ToString();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error pada hitung total: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal menghitung total: " + ex.Message);
            }
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dtMahasiswa = new DataTable();
                            da.Fill(dtMahasiswa);

                            bindingSource.DataSource = dtMahasiswa;
                            dataGridView1.DataSource = bindingSource;

                            BindControls();
                        }
                    }
                }
                HitungTotal();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error pada load data: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Gagal load data: " + ex.Message);
            }
        }

        private void BindControls()
        {
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtKodeProdi.DataBindings.Clear();

            txtNIM.DataBindings.Add("Text", bindingSource, "NIM");
            txtNama.DataBindings.Add("Text", bindingSource, "Nama");
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin");
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir");
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat");
            txtKodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi");
        }

        private void ClearForm()
        {
            bindingSource.AddNew();
            txtNIM.Focus();
        }

        // ================== EVENT HANDLERS FORM ==================

        private void FormMahasiswa_Load_1(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'dBAkademikADODataSet.Mahasiswa' table. You can move, or remove it, as needed.
            this.mahasiswaTableAdapter2.Fill(this.dBAkademikADODataSet.Mahasiswa);
            // TODO: This line of code loads data into the 'dBAkademikADODataSet11.Mahasiswa' table. You can move, or remove it, as needed.
            this.mahasiswaTableAdapter1.Fill(this.dBAkademikADODataSet11.Mahasiswa);
            cmbJK.Items.Clear();
            cmbJK.Items.Add("L");
            cmbJK.Items.Add("P");

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
            SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            SqlTransaction trans = conn.BeginTransaction();

            try
            {
                // Perintah 1: Eksekusi Stored Procedure Mahasiswa (Memicu trg_InsertMahasiswa di database)
                SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn, trans);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);
                cmd.Parameters.AddWithValue("@TanggalDaftar", DateTime.Now);

                cmd.ExecuteNonQuery();

                // Perintah 2: Eksekusi Log Manual Aplikasi (Sesuai kolom tabel LogAktivitas: aktivitas, waktu)
                // Catatan Praktikum 12: Ganti "LogAktivitas" menjadi "LogAktivitasSalah" untuk simulasi Rollback
                SqlCommand cmdLog = new SqlCommand(
                    "INSERT INTO LogAktivitas (aktivitas, waktu) VALUES (@aktivitas, GETDATE())", conn, trans);

                cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MANUAL VIA APP - NIM: " + txtNIM.Text);
                cmdLog.ExecuteNonQuery();

                // Jika kedua proses di atas sukses tanpa kendala, simpan permanen
                trans.Commit();
                MessageBox.Show("Data berhasil ditambahkan");

                LoadData();
                ClearForm();
            }
            catch (SqlException ex)
            {
                // Rollback otomatis jika ada constraint error (misal NIM duplikat) atau tabel salah
                trans.Rollback();
                SimpanLog("ROLLBACK INSERT: " + ex.Message);
                MessageBox.Show("SQL Error (Transaksi Di-Rollback): " + ex.Message);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                SimpanLog("ROLLBACK INSERT: " + ex.Message);
                MessageBox.Show("General Error (Transaksi Di-Rollback): " + ex.Message);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        // ================== ACTION BUTTONS LAINNYA ==================

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                        cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                        cmd.Parameters.AddWithValue("@KodeProdi", txtKodeProdi.Text);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data berhasil diupdate");
                LoadData();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Yakin ingin menghapus data?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NIM", SqlDbType.Char, 11).Value = txtNIM.Text;

                            connection.Open();
                            cmd.ExecuteNonQuery(); // Memicu trg_DeleteMahasiswa di database

                            MessageBox.Show("Data mahasiswa berhasil dihapus");
                        }
                    }
                    LoadData();
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error: " + ex.Message);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("Gagal Hapus: " + ex.Message);
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                        BEGIN
                            DELETE FROM dbo.Mahasiswa;
                            INSERT INTO dbo.Mahasiswa
                            SELECT * FROM dbo.Mahasiswa_Backup;
                        END";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data berhasil direset dari backup");
                LoadData();
                ClearForm();
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error saat reset: " + ex.Message);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("Reset gagal: " + ex.Message);
            }
        }

        // ====== TOMBOL UJI COBA SQL INJECTION (MEMICU TRG_PREVENTMASSUPDATE & LOGKEAMANAN) ======
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Mahasiswa SET Nama='" + txtNama.Text + "' WHERE NIM='" + txtNIM.Text + "'";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        int result = cmd.ExecuteNonQuery();
                        MessageBox.Show(result + " baris terupdate via Injeksi");
                    }
                }
                LoadData();
            }
            catch (SqlException ex)
            {
                // Jalur ini menangkap pembatalan RAISERROR dari trg_PreventMassUpdate
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error Terdeteksi:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Koneksi berhasil ke " + connection.Database);
                }
            }
            catch (Exception ex) { MessageBox.Show("Koneksi gagal: " + ex.Message); }
        }

        private void btnLoad_Click(object sender, EventArgs e) { LoadData(); }

        // ================== PLACEHOLDER CONTROLS ==================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e) { }
        private void txtKodeProdi_TextChanged(object sender, EventArgs e) { }
        private void bindingNavigatorPositionItem_Click(object sender, EventArgs e) { }
        private void lblTotal_Click(object sender, EventArgs e) { }
        private void btnUpdate_Click_2(object sender, EventArgs e) { button1_Click(sender, e); }

        private void BtnRekap_Click(object sender, EventArgs e)
        {
            RekapMahasiswa fm2 = new RekapMahasiswa();
            fm2.Show();
            this.Hide();
        }
    }
}