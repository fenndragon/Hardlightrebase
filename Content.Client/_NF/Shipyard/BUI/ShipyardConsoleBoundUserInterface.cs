// using Content.Client._Mono.Shipyard; // WOLFGATE(ShipPreview): unused now
using Content.Client._NF.Shipyard.UI;
using Content.Shared.Containers.ItemSlots;//hl
// WOLFGATE(ShipPreview): the Wolfgate ship previewer replaces the Mono preview-map flow for this button.
using Content.Client._WF.ShipPreview.UI;
using Content.Shared._WF.Traders; // WOLFGATE(Traders)
using Content.Shared.Containers.ItemSlots;
using Robust.Client.UserInterface;//hl
using Content.Shared._NF.Shipyard.BUI;
using Content.Shared._NF.Shipyard.Events;
using Content.Client.Shuttles.Save;//hl
using static Robust.Client.UserInterface.Controls.BaseButton;
using Robust.Client.UserInterface.Controls;//hl

namespace Content.Client._NF.Shipyard.BUI;

public sealed class ShipyardConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly ShipFileManagementSystem _shipFileManagementSystem = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("shipyard");

    private ShipyardConsoleMenu? _menu;
    private ShipyardRulesPopup? _rulesWindow;
    // WOLFGATE(ShipPreview): unused, the Wolfgate previewer replaces the Mono preview-map flow
    // [Dependency] private ShipyardPreviewSystem _preview = default!;
    // WOLFGATE(ShipPreview): one shared previewer window per BUI instance, reused across Preview button presses.
    private ShipPreviewWindow? _previewWindow;
    public int Balance { get; private set; }

    public int? ShipSellValue { get; private set; }
    private Button? _loadShipButton;
    private Button? _saveShipButton;
    private ItemList? _savedShipsList;
    private int _selectedShipIndex = -1;

    public ShipyardConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _menu = new ShipyardConsoleMenu(this);
        // Disable the NFSD popup for now.
        // var rules = new FormattedMessage();
        // _rulesWindow = new ShipyardRulesPopup(this);
        _menu.OpenCentered();
        // if (ShipyardConsoleUiKey.Security == (ShipyardConsoleUiKey) UiKey)
        // {
        //     rules.AddText(Loc.GetString($"shipyard-rules-default1"));
        //     rules.PushNewline();
        //     rules.AddText(Loc.GetString($"shipyard-rules-default2"));
        //     _rulesWindow.ShipRules.SetMessage(rules);
        //     _rulesWindow.OpenCentered();
        // }
        _menu.OnClose += Close;
        _menu.OnOrderApproved += ApproveOrder;
        _menu.OnSellShip += SellShip;
        _menu.OnUnassignDeed += UnassignDeed;
        _menu.OnSaveShip += SaveShip;
        _menu.OnReloadShips += ReloadShips;
        _menu.OnRenameShip += RenameShip;
        _menu.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent("ShipyardConsole-targetId"));
        _menu.OnPreviewShip += PreviewShip;

        // WOLFGATE(Traders) START: an NPC dealer only sells; the card is theirs to hold and the server refuses both buttons.
        if (EntMan.HasComponent<TraderComponent>(Owner))
        {
            _menu.HideSellControls();
            _menu.TargetIdButton.Disabled = true;
        }
        // WOLFGATE END
        InitializeSaveLoadControls();
    }

    private void InitializeSaveLoadControls()
    {
        if (_menu == null)
            return;

        var shipCount = _shipFileManagementSystem.GetSavedShipFiles().Count;
        // Only log if there are ships to avoid spam when no ships are saved
        if (shipCount > 0)
        {
            Sawmill.Debug($"InitializeSaveLoadControls: ShipFileManagementSystem has {shipCount} ships");
        }

        _loadShipButton = _menu.FindControl<Button>("LoadShipButton");
        _saveShipButton = _menu.FindControl<Button>("SaveShipButton");
        _savedShipsList = _menu.FindControl<ItemList>("SavedShipsList");

        if (_loadShipButton != null)
            _loadShipButton.OnPressed += OnLoadShipButtonPressed;
        // Save button already wired via ShipyardConsoleMenu to raise OnSaveShip, which we handle in SaveShip()
        // Avoid wiring a second handler that would incorrectly send a direct save request.
        if (_savedShipsList != null)
            _savedShipsList.OnItemSelected += OnSavedShipSelected;

        // Subscribe to ship updates
        _shipFileManagementSystem.OnShipsUpdated += RefreshSavedShipList;
        _shipFileManagementSystem.OnShipLoaded += OnShipLoaded;

        RefreshSavedShipList();
    }
    private async void OnLoadShipButtonPressed(BaseButton.ButtonEventArgs args)
    {
        // Load the currently selected ship from the saved ships list
        if (_savedShipsList == null || _selectedShipIndex < 0 || _selectedShipIndex >= _savedShipsList.Count)
        {
            Sawmill.Warning("No ship selected for loading");
            return;
        }

        var selectedItem = _savedShipsList[_selectedShipIndex];
        var filePath = (string)selectedItem.Metadata!;

        // Load ship YAML data and send via console-specific message
        try
        {
            var yamlData = await _shipFileManagementSystem.GetShipYamlData(filePath);
            if (yamlData != null)
            {
                // Send the load message through the console's BoundUserInterface system
                SendMessage(new ShipyardConsoleLoadMessage(yamlData, filePath));
                Sawmill.Info($"Sent ship load request for '{selectedItem.Text}' via console");
            }
            else
            {
                Sawmill.Error($"Failed to load YAML data for ship '{selectedItem.Text}'");
            }
        }
        catch (Exception ex)
        {
            Sawmill.Error($"Error loading ship '{selectedItem.Text}': {ex.Message}");
        }
    }

    private void OnSavedShipSelected(ItemList.ItemListSelectedEventArgs args)
    {
        // Store selected index and update Load Ship button state
        _selectedShipIndex = args.ItemIndex;
        if (_loadShipButton != null)
            _loadShipButton.Disabled = false;
    }

    private void OnShipLoaded(string shipName)
    {
        // Refresh the ship list when a ship is loaded
        RefreshSavedShipList();
        Sawmill.Debug($"Ship '{shipName}' was loaded - refreshed saved ship list");
    }

    private void RefreshSavedShipList()
    {
        if (_savedShipsList == null)
            return;
        _savedShipsList.Clear();

        var savedShipFiles = _shipFileManagementSystem.GetSavedShipFiles();
        //Logger.Info($"RefreshSavedShipList: Found {savedShipFiles.Count} ships to display");

        foreach (var filePath in savedShipFiles)
        {
            // Extract filename without extension in a sandbox-safe way
            var fileName = ExtractFileNameWithoutExtension(filePath);
            var item = _savedShipsList.AddItem(fileName);
            item.Metadata = filePath;
            //Logger.Info($"Added ship to UI list: {fileName} (path: {filePath})");
        }

        // Enable/disable load button based on available ships
        if (_loadShipButton != null)
        {
            _loadShipButton.Disabled = savedShipFiles.Count == 0;
        }
    }
    private static string ExtractFileNameWithoutExtension(string filePath)
    {
        var fileName = filePath;
        var lastSlash = filePath.LastIndexOf('/');
        if (lastSlash >= 0)
            fileName = filePath.Substring(lastSlash + 1);
        var lastBackslash = fileName.LastIndexOf('\\');
        if (lastBackslash >= 0)
            fileName = fileName.Substring(lastBackslash + 1);
        var lastDot = fileName.LastIndexOf('.');
        if (lastDot >= 0)
            fileName = fileName.Substring(0, lastDot);
        return fileName;
    }
    private void Populate(List<string> availablePrototypes, List<string> unavailablePrototypes, bool freeListings, bool validId)
    {
        if (_menu == null)
            return;

        _menu.PopulateProducts(availablePrototypes, unavailablePrototypes, freeListings, validId);
        _menu.PopulateCategories(availablePrototypes, unavailablePrototypes);
        _menu.PopulateClasses(availablePrototypes, unavailablePrototypes);
        _menu.PopulateEngines(availablePrototypes, unavailablePrototypes);
        _menu.PopulateClassicFilter(availablePrototypes, unavailablePrototypes); // WOLFGATE(OldVessels): classic ships filter
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ShipyardConsoleInterfaceState cState)
            return;

        Balance = cState.Balance;
        ShipSellValue = cState.ShipSellValue;
        var castState = (ShipyardConsoleInterfaceState) state;
        Populate(castState.ShipyardPrototypes.available, castState.ShipyardPrototypes.unavailable, castState.FreeListings, castState.IsTargetIdPresent);
        _menu?.UpdateState(castState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!disposing) return;

        _menu?.Dispose();
        // WOLFGATE(ShipPreview): close the shared previewer window along with the console menu, so it releases its preview map.
        _previewWindow?.Close();
    }

    private void ApproveOrder(ButtonEventArgs args)
    {
        if (args.Button.Parent?.Parent?.Parent is not VesselRow row || row.Vessel == null) // Mono - another .parent? - this is really fucking stupid
        {
            return;
        }

        var vesselId = row.Vessel.ID;
        SendMessage(new ShipyardConsolePurchaseMessage(vesselId));
    }

    private void SellShip(ButtonEventArgs args)
    {
        //reserved for a sanity check, but im not sure what since we check all the important stuffs on server already
        SendMessage(new ShipyardConsoleSellMessage());
    }

    private void UnassignDeed(ButtonEventArgs args)
    {
        SendMessage(new ShipyardConsoleUnassignDeedMessage());
    }

    private void RenameShip(string newName)
    {
        SendMessage(new ShipyardConsoleRenameMessage(newName));
    }

    private void PreviewShip(ButtonEventArgs args)
    {
        if (args.Button.Parent?.Parent?.Parent is not VesselRow row || row.Vessel == null) // Mono - another .parent? - this is really fucking stupid
        {
            return;
        }

        var vessel = row.Vessel;

        // WOLFGATE(ShipPreview) START: open the client-side ship previewer instead of visiting a server-side preview map.
        // SendMessage(new ShipyardConsolePreviewMessage());
        // _preview.TryPreviewGrid(vessel);
        if (_previewWindow is not { IsOpen: true })
        {
            _previewWindow = ShipPreviewWindow.Open(vessel);
            return;
        }

        _previewWindow.SetVessel(vessel);
        _previewWindow.MoveToFront();
        // WOLFGATE END
    }
}
