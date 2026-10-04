using Godot;
using System.Collections.Generic;

public partial class PropMakerDock : VBoxContainer
{
    private LineEdit sourceFolder;
    private FileDialog folderDialog;

    private LineEdit outputFolder;
    private FileDialog outputFolderDialog;

    private ItemList modelList;
    private Label modelsLabel;
    private Label selectedModelLabel;
    private Label selectedModelsForGenerationLabel;
    
    private SpinBox massInput;
    private SpinBox scaleInput;
    private SpinBox brokenScaleInput;
    private OptionButton collisionType;
    private CheckBox destructibleCheckBox;
    private CheckBox followScaleCheckBox;

    private LineEdit brokenModelPath;
    private Button brokenModelBrowseButton;
    private FileDialog brokenModelDialog;
    private ConfirmationDialog overwriteDialog;
    private PropSettings pendingSettings;
    private string pendingOutputPath = "";
    private string pendingModelPath = "";
    
    private List<string> batchModels = new();
    private int batchIndex = 0;

    private Dictionary<string, PropSettings> propSettings = new();

    private string selectedModelPath = "";

    public override void _Ready()
    {
        BuildUI();
        CreateFolderDialog();
        CreateOutputFolderDialog();
        CreateBrokenModelDialog();
        CreateOverWriteDialog();

        modelList.ItemClicked += OnModelClicked;
    }

    private void AddSectionHeader(string text)
    {
        Label label = new Label();
        label.Text = text;
        label.AddThemeFontSizeOverride("font_size", 15);

        AddChild(label);
    }

    private void AddSectionSpacing(int height = 8)
    {
        Control spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(0, height);
        AddChild(spacer);
    }

    private Button AddHelpButton(string tooltip)
    {
        Button helpButton = new Button();
        helpButton.Text = "?";
        helpButton.TooltipText = tooltip;
        helpButton.CustomMinimumSize = new Vector2(24, 24);

        return helpButton;
    }

    private void BuildUI()
    {
        // Title
        Label title = new Label();
        title.Text = "Prop Maker";
        AddChild(title);

        HSeparator separator = new HSeparator();
        AddChild(separator);

        // Source folder
        AddSectionHeader("Source folder");

        HBoxContainer sourceRow = new HBoxContainer();
        AddChild(sourceRow);

        sourceFolder = new LineEdit();
        sourceFolder.PlaceholderText = "Select model folder...";
        sourceFolder.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        sourceRow.AddChild(sourceFolder);

        Button sourceButton = new Button();
        sourceButton.Text = "Browse";
        sourceRow.AddChild(sourceButton);

        sourceButton.Pressed += OnSourceBrowsePressed;

        // Output folder
        AddSectionHeader("Output folder");

        HBoxContainer outputRow = new HBoxContainer();
        AddChild(outputRow);

        outputFolder = new LineEdit();
        outputFolder.PlaceholderText = "Select output folder...";
        outputFolder.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        outputRow.AddChild(outputFolder);

        Button outputButton = new Button();
        outputButton.Text = "Browse";
        outputRow.AddChild(outputButton);

        outputButton.Pressed += OnOutputBrowsePressed;

        // Scan button
        Button scanButton = new Button();
        scanButton.Text = "Scan Models";
        scanButton.TooltipText = "Search model in source folder.";
        AddChild(scanButton);

        scanButton.Pressed += OnScanButtonPressed;

        // Model list
        AddSectionHeader("=== MODELS ===");

        modelsLabel = new Label();
        modelsLabel.Text = "Models Found: 0";
        AddChild(modelsLabel);

        modelList = new ItemList();
        modelList.SelectMode = ItemList.SelectModeEnum.Multi;
        modelList.CustomMinimumSize = new Vector2(0, 200);
        modelList.SizeFlagsVertical = Control.SizeFlags.ShrinkBegin;
        modelList.TooltipText = "Shows how many models were found in the source folder.";
        
        StyleBoxFlat modelListStyle = new StyleBoxFlat();
        modelListStyle.BgColor = new Color(0.08f, 0.08f, 0.08f);
        modelListStyle.BorderWidthLeft = 1;
        modelListStyle.BorderWidthTop = 1;
        modelListStyle.BorderWidthRight = 1;
        modelListStyle.BorderWidthBottom = 1;
        modelListStyle.BorderColor = new Color(0.25f, 0.25f, 0.25f);
        modelListStyle.CornerRadiusTopLeft = 4;
        modelListStyle.CornerRadiusTopRight = 4;
        modelListStyle.CornerRadiusBottomLeft = 4;
        modelListStyle.CornerRadiusBottomRight = 4;

        modelList.AddThemeStyleboxOverride("panel", modelListStyle);
        AddChild(modelList);

        AddSectionSpacing();

        // Prop settings
        AddSectionHeader("=== PROP SETTINGS ===");

        selectedModelLabel = new Label();
        selectedModelLabel.Text = "Selected Model: None";
        AddChild(selectedModelLabel);

        AddSectionSpacing(4);

        // Mass
        HBoxContainer massRow = new HBoxContainer();
        AddChild(massRow);

        Label massLabel = new Label();
        massLabel.Text = "Mass:";
        massRow.AddChild(massLabel);

        massInput = new SpinBox();
        massInput.MinValue = 0.1;
        massInput.MaxValue = 1000.0;
        massInput.Step = 0.1;
        massInput.Value = 1.0;
        massInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        massInput.TooltipText = "Sets the mass of the generated prop in kilograms(kg).";
        massRow.AddChild(massInput);

        // Scale
        HBoxContainer scaleRow = new HBoxContainer();
        AddChild(scaleRow);

        Label scaleLabel = new Label();
        scaleLabel.Text = "Scale:";
        scaleRow.AddChild(scaleLabel);

        scaleInput = new SpinBox();
        scaleInput.MinValue = 0.01;
        scaleInput.MaxValue = 100.0;
        scaleInput.Step = 0.01;
        scaleInput.Value = 1.0;
        scaleInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        scaleInput.TooltipText = "Sets the scale of the generated prop.";
        scaleRow.AddChild(scaleInput);

        // Collision
        HBoxContainer collisionRow = new HBoxContainer();
        AddChild(collisionRow);

        Label collisionLabel = new Label();
        collisionLabel.Text = "Collision:";
        collisionRow.AddChild(collisionLabel);

        collisionType = new OptionButton();
        collisionType.AddItem("Box");
        collisionType.AddItem("Sphere");
        collisionType.AddItem("Capsule");
        collisionType.AddItem("Convex");

        collisionType.TooltipText = "Selects the type of collision shape generated for the prop.";

        collisionRow.AddChild(collisionType);

        // Destructible
        HBoxContainer destructibleRow = new HBoxContainer();
        AddChild(destructibleRow);
        
        destructibleCheckBox = new CheckBox();
        destructibleCheckBox.Text = "Destructible";
        destructibleCheckBox.TooltipText = "If enabled, the generated prop can use a broken model when destroyed.";
        destructibleRow.AddChild(destructibleCheckBox);

        // Broken model
        HBoxContainer brokenModelRow = new HBoxContainer();

        Label brokenModelLabel = new Label();
        brokenModelLabel.Text = "Broken Model:";

        brokenModelPath = new LineEdit();
        brokenModelPath.Editable = false;
        brokenModelPath.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        brokenModelPath.TooltipText = "Selects the model that will be use when this prop is broken";

        brokenModelBrowseButton = new Button();
        brokenModelBrowseButton.Text = "Browse";

        brokenModelRow.AddChild(brokenModelLabel);
        brokenModelRow.AddChild(brokenModelPath);
        brokenModelRow.AddChild(brokenModelBrowseButton);

        AddChild(brokenModelRow);

        brokenModelBrowseButton.Disabled = true;
        destructibleCheckBox.Toggled += OnDestructibleToggled;

        // Follow prop scale
        HBoxContainer followScaleRow = new HBoxContainer();
        AddChild(followScaleRow);

        followScaleCheckBox = new CheckBox();
        followScaleCheckBox.Text = "Follow Prop Scale";
        followScaleCheckBox.ButtonPressed = true;
        followScaleCheckBox.TooltipText = "If enabled, the broken model will use the same scale as the prop.";

        followScaleRow.AddChild(followScaleCheckBox);

        HBoxContainer brokenScaleRow = new HBoxContainer();
        AddChild(brokenScaleRow);

        Label brokenScaleLabel = new Label();
        brokenScaleLabel.Text = "Broken Model Scale:";
        brokenScaleRow.AddChild(brokenScaleLabel);

        brokenScaleInput = new SpinBox();
        brokenScaleInput.MinValue = 0.01;
        brokenScaleInput.MaxValue = 100.0;
        brokenScaleInput.Step = 0.01;
        brokenScaleInput.Value = 1.0;
        brokenScaleInput.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        brokenScaleInput.TooltipText = "Sets the scale of the broken model when Follow Prop Scale is disabled.";

        brokenScaleRow.AddChild(brokenScaleInput);
        
        brokenScaleInput.Editable = false;
        followScaleCheckBox.Toggled += OnFollowScaleToggled;

        // Apply
        Button applyButton = new Button();
        applyButton.Text = "Apply Settings";
        applyButton.TooltipText = "Save the current prop setting for the selected model.";
        AddChild(applyButton);

        applyButton.Pressed += OnApplySettingButtonPressed;

        AddSectionSpacing();

        // Generate button
        AddSectionHeader("=== GENERATE PROP ===");

        Button generateButton = new Button();
        generateButton.Text = "Generate Selected";
        generateButton.TooltipText = "Generates prop scene for all selected models using their saved settings.";
        AddChild(generateButton);

        generateButton.Pressed += OnGenerateButtonPressed;

        selectedModelsForGenerationLabel = new Label();
        selectedModelsForGenerationLabel.Text = "Selected Models: None";
        selectedModelsForGenerationLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        AddChild(selectedModelsForGenerationLabel);
    }

    private void CreateFolderDialog()
    {
        folderDialog = new FileDialog();

        folderDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
        folderDialog.Access = FileDialog.AccessEnum.Filesystem;
        folderDialog.Title = "Select Source Model Folder";

        AddChild(folderDialog);

        folderDialog.DirSelected += OnFolderSelected;
    }

    private void CreateOutputFolderDialog()
    {
        outputFolderDialog = new FileDialog();

        outputFolderDialog.FileMode = FileDialog.FileModeEnum.OpenDir;
        outputFolderDialog.Access = FileDialog.AccessEnum.Filesystem;
        outputFolderDialog.Title = "Select Output Folder";

        AddChild(outputFolderDialog);

        outputFolderDialog.DirSelected += OnOutputFolderSelected;
    }

    private void OnModelClicked(long index, Vector2 atPosition, long mouseButtonIndex)
    {
        string fileName = modelList.GetItemText((int)index);

        selectedModelPath = sourceFolder.Text.PathJoin(fileName);

        selectedModelLabel.Text = $"Selected Model: {fileName}";

        GD.Print($"Selected model: {fileName}");
        GD.Print($"Selected model path: {selectedModelPath}");

        if (!propSettings.TryGetValue(
            selectedModelPath,
            out PropSettings settings))
        {
            GD.PrintErr($"No settings found for: {selectedModelPath}");
            return;
        }

        massInput.Value = settings.Mass;
        scaleInput.Value = settings.Scale;
        destructibleCheckBox.ButtonPressed = settings.IsDestructible;
        brokenModelPath.Text = settings.BrokenModelPath;
        brokenScaleInput.Value = settings.BrokenModelScale;
        followScaleCheckBox.ButtonPressed = settings.FollowBrokenModelScale;

        for (int i = 0; i < collisionType.ItemCount; i++)
        {
            if (collisionType.GetItemText(i) == settings.CollisionType)
            {
                collisionType.Select(i);
                break;
            }
        }

        // Update selected models for generation
        int[] selectedItems = modelList.GetSelectedItems();

        if (selectedItems.Length == 0)
        {
            selectedModelsForGenerationLabel.Text = "Selected Models: None";
        }
        else
        {
            string selectedModelsText = $"Selected Models: {selectedItems.Length}\n";

            foreach (int selectedIndex in selectedItems)
            {
                selectedModelsText += $"{modelList.GetItemText(selectedIndex)}\n";
            }

            selectedModelsForGenerationLabel.Text = selectedModelsText;
        }
    }

    private void OnSourceBrowsePressed()
    {
        GD.Print("Opening source folder dialog...");

        folderDialog.PopupCentered();
    }

    private void OnOutputBrowsePressed()
    {
        GD.Print("Opening output folder dialog...");

        outputFolderDialog.PopupCentered();
    }

    private void OnFolderSelected(string path)
    {
        GD.Print($"Selected folder: {path}");

        sourceFolder.Text = path;
    }

    private void OnOutputFolderSelected(string path)
    {
        GD.Print($"Selected output folder: {path}");

        outputFolder.Text = path;
    }

    private void OnScanButtonPressed()
    {
        string path = sourceFolder.Text;

        if (string.IsNullOrEmpty(path))
        {
            GD.Print("No source folder selected!");
            return;
        }

        GD.Print($"Scanning: {path}");

        modelList.Clear();

        DirAccess dir = DirAccess.Open(path);

        if (dir == null)
        {
            GD.PrintErr($"Could not open folder: {path}");
            return;
        }

        dir.ListDirBegin();

        int modelCount = 0;
        string fileName = dir.GetNext();

        while (!string.IsNullOrEmpty(fileName))
        {
            if (!dir.CurrentIsDir())
            {
                string extension = fileName.GetExtension().ToLower();

                if (extension == "glb" || extension == "gltf")
                {
                    GD.Print($"Found model: {fileName}");

                    string modelPath = path.PathJoin(fileName);
                    string resourcePath = GetResourcePath(modelPath);

                    GD.Print($"Filesystem: {modelPath}");
                    GD.Print($"Resource: {resourcePath}");

                    modelList.AddItem(fileName);

                    propSettings[modelPath] = new PropSettings(modelPath);

                    modelCount++;
                }
            }

            fileName = dir.GetNext();
        }

        dir.ListDirEnd();

        modelsLabel.Text = $"Models Found: {modelCount}";

        GD.Print($"Found {modelCount} model(s).");
    }

    private void OnDestructibleToggled(bool enabled)
    {
        brokenModelBrowseButton.Disabled = !enabled;

        if (!enabled)
        {
            brokenModelBrowseButton.Text = "Browse";
        }
    }

    private void OnFollowScaleToggled(bool enabled)
    {
        brokenScaleInput.Editable = !enabled;
    }

    private void CreateBrokenModelDialog()
    {
        brokenModelDialog = new FileDialog();

        brokenModelDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        brokenModelDialog.Access = FileDialog.AccessEnum.Filesystem;

        brokenModelDialog.Filters = new string[]
        {
            "*.glb, *.gltf, *.tscn"
        };

        AddChild(brokenModelDialog);

        brokenModelDialog.FileSelected += OnBrokenModelSelected;
        brokenModelBrowseButton.Pressed += OnBrokenModelBrowsePressed;
    }

    private void OnBrokenModelBrowsePressed()
    {
        brokenModelDialog.PopupCenteredRatio();
    }

    private void OnBrokenModelSelected(string path)
    {
        brokenModelPath.Text = path;

        GD.Print($"Selected broken model: {path}");
    }

    private void OnApplySettingButtonPressed()
    {
        if (string.IsNullOrEmpty(selectedModelPath))
        {
            GD.PrintErr("No model selected!");
            return;
        }

        PropSettings settings = propSettings[selectedModelPath];

        settings.Mass = massInput.Value;
        settings.Scale = scaleInput.Value;
        settings.CollisionType = collisionType.GetItemText(collisionType.Selected);
        settings.IsDestructible = destructibleCheckBox.ButtonPressed;
        settings.BrokenModelPath = brokenModelPath.Text;

        settings.FollowBrokenModelScale = followScaleCheckBox.ButtonPressed;
        settings.BrokenModelScale = brokenScaleInput.Value;

        GD.Print(
            $"Updated {selectedModelPath}: " + 
            $"Mass={settings.Mass}, " + 
            $"Scale={settings.Scale}" + 
            $"Collision={settings.CollisionType}, " + 
            $"Destructible={settings.IsDestructible}, " + 
            $"Brokenmodel={settings.BrokenModelPath}" + 
            $"FollowScale={settings.FollowBrokenModelScale}");
    }
    
    private void OnGenerateButtonPressed()
    {
        int[] selectedItems = modelList.GetSelectedItems();

        if (selectedItems.Length == 0)
        {
            GD.PrintErr("No models selected!");
            return;
        }

        if (string.IsNullOrEmpty(outputFolder.Text))
        {
            GD.PrintErr("No output folder selected!");
            return;
        }
        
        batchModels.Clear();
        batchIndex = 0;

        foreach (int index in selectedItems)
        {
            string fileName = modelList.GetItemText(index);
            batchModels.Add(fileName);
        }

        GD.Print($"Starting batch generation: {batchModels.Count} models.");

        GenerateNextBatchModel();
    }

    private void GenerateNextBatchModel()
    {
        if (batchIndex >= batchModels.Count)
        {
            GD.Print("=== Batch generation complete ===");

            batchModels.Clear();
            batchIndex = 0;

            return;
        }

        string fileName = batchModels[batchIndex];
        string modelPath = sourceFolder.Text.PathJoin(fileName);

        GD.Print($"=== Generating model {batchIndex + 1}/{batchModels.Count}: {fileName} ===");

        if (!propSettings.TryGetValue(modelPath, out PropSettings settings))
        {
            GD.PrintErr($"No setting found for: {modelPath}");

            batchIndex ++;
            GenerateNextBatchModel();
            return;
        }

        if (settings.IsDestructible && string.IsNullOrEmpty(settings.BrokenModelPath))
        {
            GD.PrintErr($"Destructible model has no broken model selected: {fileName}");
            
            batchIndex ++;
            GenerateNextBatchModel();
            return;
        }

        string fileNameWithoutExtension = modelPath.GetFile().GetBaseName();

        string outputPath = GetResourcePath(outputFolder.Text.PathJoin(fileNameWithoutExtension + ".tscn"));

        if (ResourceLoader.Exists(outputPath))
        {
            GD.Print($"Output already exists: {outputPath}");
            pendingModelPath = modelPath;
            pendingSettings = settings;
            pendingOutputPath = outputPath;

            overwriteDialog.DialogText = $"The file '{fileNameWithoutExtension}.tscn' already exists.\n\nOverwrite it?";

            overwriteDialog.PopupCentered();
            return;
        }

        string resourcePath = GetResourcePath(modelPath);

        if (string.IsNullOrEmpty(resourcePath))
        {
            GD.Print("External model detected.");

            string copiedPath = ImportExternalResource(modelPath);

            if (string.IsNullOrEmpty(copiedPath))
            {    
                batchIndex ++;
                GenerateNextBatchModel();
                return;
            }

            string copiedResourcePath = GetResourcePath(copiedPath);

            EditorInterface.Singleton.GetResourceFilesystem().Scan();

            GD.Print($"Copied resource path: {copiedResourcePath}");
            GD.Print($"Resource exists: {ResourceLoader.Exists(copiedResourcePath)}");

            GenerateProp(copiedResourcePath, settings);
        }
        else
        {
            GD.Print($"Model is already inside project: {resourcePath}");

            GenerateProp(resourcePath, settings);
        }

    }

    private void GenerateProp(string modelPath, PropSettings settings)
    {
        GD.Print($"Trying to load: {modelPath}");

        PackedScene modelScene = GD.Load<PackedScene>(modelPath);

        if (modelScene == null)
        {
            GD.PrintErr($"Could not load model: {modelPath}");
            return;
        }

        Node3D model = modelScene.Instantiate<Node3D>();

        GD.Print($"Successfully loaded model: {model.Name}");

        // Create RigidBody

        RigidBody3D rigidBody = settings.IsDestructible ? new DestructibleProp() : new RigidBody3D();

        rigidBody.Name = model.Name;
        rigidBody.Mass = (float) settings.Mass;
        GD.Print($"Prop scale setting: {settings.Scale}");

        if (settings.IsDestructible)
        {
            if (rigidBody is DestructibleProp destructible)
            {
                if (string.IsNullOrEmpty(settings.BrokenModelPath))
                {
                    GD.PrintErr($"No broken model selected for: {model.Name}");
                    rigidBody.QueueFree();
                    return;
                }

                string brokenModelResourcePath = GetResourcePath(settings.BrokenModelPath);
                
                if (string.IsNullOrEmpty(brokenModelResourcePath))
                {
                    GD.Print("Broken model is external.");

                    string copiedPath = ImportExternalResource(settings.BrokenModelPath);

                    if (string.IsNullOrEmpty(copiedPath))
                    {
                        GD.PrintErr("Failed to import external broken model.");
                        rigidBody.QueueFree();
                        return;
                    }

                    brokenModelResourcePath = GetResourcePath(copiedPath);
                }

                string brokenModelExtension = brokenModelResourcePath.GetExtension().ToLower();
                
                if (brokenModelExtension == "glb")
                {
                    GD.Print("Broken model is GLB. Generating TSCN.");

                    string brokenModelName = brokenModelResourcePath.GetFile().GetBaseName();

                    string brokenModelOutputPath = GetResourcePath(outputFolder.Text.PathJoin(brokenModelName + ".tscn"));

                    if (!ResourceLoader.Exists(brokenModelOutputPath))
                    {
                        double brokenModelScale = settings.FollowBrokenModelScale ? settings.Scale : settings.BrokenModelScale;

                        bool generated = GenerateBrokenModel(brokenModelResourcePath, brokenModelOutputPath, settings.Scale);

                        if (!generated)
                        {
                            GD.PrintErr("Failed to generate broken model TSCN.");
                            rigidBody.QueueFree();
                            return;
                        }
                    }
                    else
                    {
                        GD.Print($"Broken model TSCN already exists: {brokenModelOutputPath}");
                    }

                    brokenModelResourcePath = brokenModelOutputPath;
                }

                PackedScene brokenPrefab = GD.Load<PackedScene>(brokenModelResourcePath);

                if (brokenPrefab == null)
                {
                    GD.PrintErr($"Could not load broken model: {brokenModelResourcePath}");
                    rigidBody.QueueFree();
                    return;
                }

                destructible.BrokenPrefab = brokenPrefab;

                GD.Print($"Assigned broken prefab: {brokenModelResourcePath}");
            }
        }

        GD.Print($"Created RigidBody3D: {rigidBody.Name}");
        GD.Print($"Set mass: {rigidBody.Mass}");

        rigidBody.AddChild(model);
        model.Owner = rigidBody;

        GD.Print($"Added model: {model.Name}");

        // Create Collision
        CollisionShape3D collisionShape = new CollisionShape3D();
        collisionShape.Name = "CollisionShape3D";

        switch(settings.CollisionType)
        {
            case "Box":
                {
                    Aabb modelAabb = GetModelAabb(model);

                    BoxShape3D boxShape = new BoxShape3D();
                    boxShape.Size = modelAabb.Size;

                    collisionShape.Shape = boxShape;
                    collisionShape.Position = modelAabb.GetCenter();

                    GD.Print($"Box size: {boxShape.Size}");
                    GD.Print($"Box position: {collisionShape.Position}");

                    break;
                }
                

            case "Sphere":
                {
                    Aabb modelAabb = GetModelAabb(model);

                    SphereShape3D sphereShape = new SphereShape3D();
                    
                    Vector3 size = modelAabb.Size;

                    float largestDimension = Mathf.Max(size.X, Mathf.Max(size.Y, size.Z));

                    sphereShape.Radius = largestDimension / 2.0f;

                    collisionShape.Shape = sphereShape;
                    collisionShape.Position = modelAabb.GetCenter();

                    GD.Print($"Sphere radius: {sphereShape.Radius}");
                    GD.Print($"Sphere position: {collisionShape.Position}");

                    break;
                }
                
            
            case "Capsule":
                {
                    Aabb modelAabb = GetModelAabb(model);

                    Vector3 size = modelAabb.Size;

                    float radius = Mathf.Max(size.X, size.Z) / 2.0f;
                    float height = Mathf.Max(size.Y, radius * 2.0f);

                    CapsuleShape3D capsuleShape = new CapsuleShape3D();

                    capsuleShape.Radius = radius;
                    capsuleShape.Height = height;

                    collisionShape.Shape = capsuleShape;
                    collisionShape.Position = modelAabb.GetCenter();

                    GD.Print($"Capsule radius: {capsuleShape.Radius}");
                    GD.Print($"Capsule height: {capsuleShape.Height}");
                    GD.Print($"Capsule position: {collisionShape.Position}");

                    break;
                }

            case "Convex":
                {
                    ConvexPolygonShape3D convexShape = CreateConvexShape(model);
                    
                    if (convexShape == null)
                    {
                        GD.PushError("Could not create convex collision.");
                        rigidBody.QueueFree();
                        return;
                    }

                    collisionShape.Shape = convexShape;
                    GD.Print($"Created convex collision.");

                    break;
                }
                   

            default:
                GD.PrintErr($"Unsupported collision type: {settings.CollisionType}");
                rigidBody.QueueFree();
                return;
        }

        rigidBody.AddChild(collisionShape);
        collisionShape.Owner = rigidBody;

        Vector3 propScale = Vector3.One * (float)settings.Scale;

        model.Scale = propScale;
        collisionShape.Scale = propScale;

        GD.Print($"Created collision: {settings.CollisionType}");

        SavePropScene(rigidBody, settings);
    }

    private ConvexPolygonShape3D CreateConvexShape(Node3D model)
    {
        List<Vector3> vertices = new();

        foreach (Node node in model.FindChildren("*", "MeshInstance3D", true, false))
        {
            MeshInstance3D meshInstance = node as MeshInstance3D;

            if (meshInstance == null || meshInstance.Mesh == null)
                continue;
            
            ArrayMesh arrayMesh = meshInstance.Mesh as ArrayMesh;

            if (arrayMesh == null)
                continue;
            
            for (int surface = 0; surface < arrayMesh.GetSurfaceCount(); surface++)
            {
                Godot.Collections.Array arrays = arrayMesh.SurfaceGetArrays(surface);

                if (arrays.Count <= (int)Mesh.ArrayType.Vertex)
                    continue;
                
                Godot.Collections.Array vertextArray = (Godot.Collections.Array)arrays[(int)Mesh.ArrayType.Vertex];

                for (int i = 0; i < vertextArray.Count; i++)
                {
                    Vector3 vertex = (Vector3)vertextArray[i];

                    vertex = meshInstance.Transform * vertex;

                    vertices.Add(vertex);
                }
            }
        }

        if (vertices.Count == 0)
        {
            GD.PrintErr("No mesh verticies found.");
            return null;
        }

        ConvexPolygonShape3D shape = new ConvexPolygonShape3D();

        shape.Points = vertices.ToArray();

        GD.Print($"Convex collision vertices: {vertices.Count}");

        return shape;
    }

    private void SavePropScene(RigidBody3D rigidBody, PropSettings settings)
    {
        string fileName = settings.ModelPath.GetFile().GetBaseName() + ".tscn";

        string outputPath = GetResourcePath(outputFolder.Text.PathJoin(fileName));

        GD.Print($"Saving prop to: {outputPath}");

        SavePropSceneNow(rigidBody, settings, outputPath);
    }

    private Aabb GetModelAabb(Node3D model)
    {
        Aabb? combinedAabb = null;

        foreach (Node node in model.FindChildren("*", "MeshInstance3D", true, false))
        {
            MeshInstance3D meshInstance = node as MeshInstance3D;

            if (meshInstance == null || meshInstance.Mesh == null)
                continue;
            
            Aabb meshAabb = meshInstance.GetAabb();

            // Convert mesh AABB into model-local space
            Transform3D transform = meshInstance.Transform;
            meshAabb = transform * meshAabb;

            if (combinedAabb == null)
                combinedAabb = meshAabb;
            else
                combinedAabb = combinedAabb.Value.Merge(meshAabb);
        }

        return combinedAabb ?? new Aabb(Vector3.Zero, Vector3.One);
    }

    private string ImportExternalResource(string sourcePath)
    {
        string fileName = sourcePath.GetFile();
        string destinationDir = ProjectSettings.GlobalizePath("res://PropMakerSources");
        string destinationPath = destinationDir.PathJoin(fileName);

        DirAccess.MakeDirRecursiveAbsolute(destinationDir);

        Error error = DirAccess.CopyAbsolute(sourcePath, destinationPath);

        if (error != Error.Ok)
        {
            GD.PrintErr($"Failed to copy resource: {sourcePath}");
            GD.PrintErr($"Error: {error}");
            return "";
        }

        GD.Print($"Copied external resource: {sourcePath}");
        GD.Print($"Destination: {destinationPath}");

        return destinationPath;
    }

    private string GetResourcePath(string filePath)
    {
        string projectPath = ProjectSettings.GlobalizePath("res://");

        filePath = filePath.Replace("\\", "/");
        projectPath = projectPath.Replace("\\", "/");

        if (filePath.StartsWith(projectPath))
        {
            string relativePath = filePath.Substring(projectPath.Length);

            return "res://" + relativePath;
        }

        return "";
    }

    private void CreateOverWriteDialog()
    {
        overwriteDialog = new ConfirmationDialog();

        overwriteDialog.Title = "Overwrite Prop?";
        overwriteDialog.DialogText = "The output file already exists. Do you want to overwrite it?";

        AddChild(overwriteDialog);

        overwriteDialog.Confirmed += OnOverwriteConfirmed;
        overwriteDialog.Canceled += OnOverwriteCanceled;
    }

    private void OnOverwriteConfirmed()
    {
        GD.Print($"Overwriting existing prop: {pendingOutputPath}");

        string modelPath = pendingModelPath;
        PropSettings settings = pendingSettings;

        pendingModelPath = "";
        pendingSettings = null;
        pendingOutputPath = "";

        string resourcePath = GetResourcePath(modelPath);

        if (string.IsNullOrEmpty(resourcePath))
        {
            GD.Print("External model detected.");

            string copiedPath = ImportExternalResource(modelPath);

            if (string.IsNullOrEmpty(copiedPath))
            {
                batchIndex++;
                GenerateNextBatchModel();
                return;
            }

            resourcePath = GetResourcePath(copiedPath);
        }

        GenerateProp(resourcePath, settings);
    }

    private void OnOverwriteCanceled()
    {
        GD.Print("Overwrite canceled.");

        pendingModelPath = "";
        pendingSettings = null;
        pendingOutputPath = "";

        batchIndex++;
        GenerateNextBatchModel();
    }

    private void SavePropSceneNow(RigidBody3D rigidBody, PropSettings settings, string outputPath)
    {
        PackedScene packedScene = new PackedScene();

        Error packError = packedScene.Pack(rigidBody);

        if (packError != Error.Ok)
        {
            GD.PrintErr($"Failed to pack prop: {packError}");
            rigidBody.QueueFree();
            return;
        }

        Error saveError = ResourceSaver.Save(packedScene, outputPath);

        if (saveError != Error.Ok)
        {
            GD.PrintErr($"Failed to save prop: {saveError}");
            rigidBody.QueueFree();
            return;
        }

        GD.Print($"Successfully saved prop: {outputPath}");

        EditorInterface.Singleton.GetResourceFilesystem().Scan();

        GD.Print("Requested Godot filesystem scan.");

        rigidBody.QueueFree();

        batchIndex++;
        GenerateNextBatchModel();
    }

    private bool IsGlbFile(string path)
    {
        return path.GetExtension().ToLower() == "glb";
    }

    private bool GenerateBrokenModel(string modelPath, string outputPath, double brokenModelScale)
    {
        GD.Print($"=== Generating Broken Model: {modelPath} ===");

        PackedScene modelScene = GD.Load<PackedScene>(modelPath);

        if (modelScene == null)
        {
            GD.PrintErr($"Could not load broken model: {modelPath}");
            return false;
        }

        Node3D importedModel = modelScene.Instantiate<Node3D>();

        if (importedModel == null)
        {
            GD.PrintErr($"Could not instantiate broken model: {modelPath}");
            return false;
        }

        GD.Print($"Loaded broken model: {importedModel.Name}");

        Node3D brokenModelRoot = new Node3D();
        brokenModelRoot.Name = importedModel.Name;
        
        brokenModelRoot.Scale = Vector3.One * (float)brokenModelScale;
        
        int meshCount = 0;

        foreach (Node child in importedModel.GetChildren())
        {
            if (child is not MeshInstance3D mesh)
                continue;

            meshCount++;

            GD.Print($"Found broken piece: {mesh.Name}");

            RigidBody3D rigidBody = new RigidBody3D();
            rigidBody.Name = mesh.Name;
            rigidBody.Transform = mesh.Transform;

            MeshInstance3D meshCopy = new MeshInstance3D();
            meshCopy.Name = mesh.Name;
            meshCopy.Mesh = mesh.Mesh;

            rigidBody.AddChild(meshCopy);

            // Create convex collision from this mesh
            ConvexPolygonShape3D convexShape = null;

            if (mesh.Mesh != null)
            {
                convexShape = mesh.Mesh.CreateConvexShape();
            }

            if (convexShape == null)
            {
                GD.PrintErr($"Could not create convex collision for: {mesh.Name}");

                importedModel.QueueFree();
                brokenModelRoot.QueueFree();

                return false;
            }

            CollisionShape3D collisionShape = new CollisionShape3D();
            collisionShape.Name = "CollisionShape3D";
            collisionShape.Shape = convexShape;

            rigidBody.AddChild(collisionShape);

            brokenModelRoot.AddChild(rigidBody);

            rigidBody.Owner = brokenModelRoot;
            meshCopy.Owner = brokenModelRoot;
            collisionShape.Owner = brokenModelRoot;

            GD.Print($"Created RigidBody3D: {rigidBody.Name}");
            GD.Print($"Created convex collision for: {rigidBody.Name}");
        }

        if (meshCount == 0)
        {
            GD.PrintErr("No MeshInstance3D pieces found in broken model.");

            importedModel.QueueFree();
            brokenModelRoot.QueueFree();

            return false;
        }

        PackedScene packedScene = new PackedScene();

        Error packError = packedScene.Pack(brokenModelRoot);

        if (packError != Error.Ok)
        {
            GD.PrintErr($"Failed to pack broken model: {packError}");

            importedModel.QueueFree();
            brokenModelRoot.QueueFree();

            return false;
        }

        Error saveError = ResourceSaver.Save(packedScene, outputPath);

        if (saveError != Error.Ok)
        {
            GD.PrintErr($"Failed to save broken model: {saveError}");

            importedModel.QueueFree();
            brokenModelRoot.QueueFree();

            return false;
        }

        GD.Print($"Successfully saved broken model: {outputPath}");

        importedModel.QueueFree();
        brokenModelRoot.QueueFree();

        EditorInterface.Singleton.GetResourceFilesystem().Scan();

        return true;
    }
}
