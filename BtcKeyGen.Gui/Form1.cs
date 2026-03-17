using System.Diagnostics;
using BtcKeyGen.Blockchain;
using BtcKeyGen.Core;
using BtcKeyGen.Crypto;

namespace BtcKeyGen.Gui;

public partial class Form1 : Form
{
    private readonly IKeyGenerator _keyGenerator = new SimulatedBtcKeyGenerator();
    private readonly IBlockchainViewer _blockchainViewer = new SimulatedBlockchainViewer();
    private readonly BindingSource _binding = new();
    private readonly List<GeneratedKey> _keys = new();

    private DataGridView _grid = null!;
    private TextBox _label = null!;
    private Button _generate = null!;
    private Button _copyPriv = null!;
    private Button _copyAddr = null!;
    private Button _view = null!;
    private Label _status = null!;

    public Form1()
    {
        InitializeComponent();
        BuildUi();
        BindData();
    }

    private void BuildUi()
    {
        Text = "BTC Keypair Generator v2.1";
        MinimumSize = new Size(900, 520);

        var top = new Panel { Dock = DockStyle.Top, Height = 54, Padding = new Padding(12, 10, 12, 10) };
        var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 36, Padding = new Padding(12, 6, 12, 6) };

        _label = new TextBox { PlaceholderText = "Label (optional)", Width = 260 };
        _generate = new Button { Text = "Generate", Width = 110, Height = 28 };
        _copyPriv = new Button { Text = "Copy Private", Width = 110, Height = 28, Enabled = false };
        _copyAddr = new Button { Text = "Copy Address", Width = 110, Height = 28, Enabled = false };
        _view = new Button { Text = "View on Explorer", Width = 130, Height = 28, Enabled = false };

        _generate.Click += (_, _) => GenerateKey();
        _copyPriv.Click += (_, _) => CopySelected(privateKey: true);
        _copyAddr.Click += (_, _) => CopySelected(privateKey: false);
        _view.Click += async (_, _) => await ViewSelectedAsync();

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        flow.Controls.Add(_label);
        flow.Controls.Add(_generate);
        flow.Controls.Add(new Label { Width = 14 });
        flow.Controls.Add(_copyPriv);
        flow.Controls.Add(_copyAddr);
        flow.Controls.Add(_view);
        top.Controls.Add(flow);

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoGenerateColumns = false
        };
        _grid.SelectionChanged += (_, _) => UpdateSelectionState();

        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Created", DataPropertyName = nameof(GeneratedKey.CreatedAt), Width = 170 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Label", DataPropertyName = nameof(GeneratedKey.Label), Width = 160 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Address (simulated)", DataPropertyName = nameof(GeneratedKey.Address), AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

        main.Controls.Add(_grid);

        _status = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
        bottom.Controls.Add(_status);

        Controls.Add(main);
        Controls.Add(bottom);
        Controls.Add(top);

        _status.Text = "Ready. Keys are simulated for safe TTP emulation.";
    }

    private void BindData()
    {
        _binding.DataSource = _keys;
        _grid.DataSource = _binding;
    }

    private void GenerateKey()
    {
        var key = _keyGenerator.Generate(_label.Text);
        _keys.Insert(0, key);
        _binding.ResetBindings(false);
        _grid.ClearSelection();
        if (_grid.Rows.Count > 0)
            _grid.Rows[0].Selected = true;
        _status.Text = $"Generated {key.Address}";
    }

    private GeneratedKey? GetSelected()
    {
        if (_grid.SelectedRows.Count != 1) return null;
        return _grid.SelectedRows[0].DataBoundItem as GeneratedKey;
    }

    private void UpdateSelectionState()
    {
        var has = GetSelected() is not null;
        _copyPriv.Enabled = has;
        _copyAddr.Enabled = has;
        _view.Enabled = has;
    }

    private void CopySelected(bool privateKey)
    {
        var key = GetSelected();
        if (key is null) return;

        Clipboard.SetText(privateKey ? key.PrivateKeyHex : key.Address);
        _status.Text = privateKey ? "Private key copied (simulated)." : "Address copied (simulated).";
    }

    private async Task ViewSelectedAsync()
    {
        var key = GetSelected();
        if (key is null) return;

        var info = await _blockchainViewer.GetAddressInfoAsync(key.Address);
        var url = _blockchainViewer.GetExplorerUrl(key.Address);
        _status.Text = $"Simulated chain info: balance {info.SimulatedBalanceBtc} BTC, tx {info.SimulatedTxCount}. Opening explorer...";

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
