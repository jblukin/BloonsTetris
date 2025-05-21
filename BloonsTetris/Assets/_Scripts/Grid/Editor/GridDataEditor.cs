using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GridDataEditorWindow : EditorWindow
{

    private const string GRIDDATA_FOLDER_PATH = "Assets/_Scripts/Grid/GridData Objs/";

    private VisualElement _inspectorPanel, _rightView;

    private ListView _leftView;

    private static GridData _gridData;

    private List<GridData> _allGridDataObjs;

    private void CreateGUI()
    {

        var GridDataGUIDs = AssetDatabase.FindAssets( "t:GridData" );

        _allGridDataObjs = new();

        foreach ( var guid in GridDataGUIDs )
            _allGridDataObjs.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( guid ) ) );

        if ( _gridData != null && _allGridDataObjs.Count > 0 )
            _gridData = _allGridDataObjs.FirstOrDefault();

        var splitView = new TwoPaneSplitView( 0, 240f, TwoPaneSplitViewOrientation.Horizontal );
        rootVisualElement.Add( splitView );

        _leftView = new ListView
        {
            makeItem = () => new Label(),
            bindItem = ( item, idx ) => { ( item as Label ).text = _allGridDataObjs[ idx ].name; },
            itemsSource = _allGridDataObjs,
            selectionType = SelectionType.Single,
            virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
            fixedItemHeight = 25,
            showAddRemoveFooter = true,
            style =
            {

                flexGrow = 1,
                flexShrink = 1

            }
        };
        splitView.Add( _leftView );

        _leftView.selectionChanged += OnGridDataSelectionChange;
        _leftView.onAdd = view =>
        {
            var gridData = CreateNewGridData();
            if ( gridData == null )
                return;
            view.itemsSource.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( gridData.name )[ 0 ] ) ) );
            view.RefreshItems();
            view.SetSelection( view.itemsSource.Count );
            view.ScrollToItem( view.itemsSource.Count );

        };

        _leftView.onRemove = view =>
        {

            if ( view.itemsSource.Count == 0 )
                return;
            GridData asset = view.selectedItem as GridData;
            view.itemsSource.RemoveAt( view.selectedIndex );
            view.RefreshItems();
            if ( view.selectedIndex == 0 )
                view.selectedIndex++;
            view.SetSelection( --view.selectedIndex );
            view.ScrollToItem( view.selectedIndex );
            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath( asset ) );

        };

        _rightView = new VisualElement();
        splitView.Add( _rightView );

        DrawRightView();

        _leftView.schedule.Execute( () => _leftView.SetSelection( _leftView.itemsSource.IndexOf( _gridData ) ) );

    }

    private void DrawRightView()
    {

        _rightView.Clear();

        _inspectorPanel ??= new InspectorElement();

        _rightView.Add( _inspectorPanel );

        _rightView.Add( new Button( () =>
        {

            if ( _leftView.selectedItem != null )
            {
                _inspectorPanel = new InspectorElement( _leftView.selectedItem as GridData );
                DrawRightView();
            }
        } )
        {
            text = "Refresh Grid",
            style =
            {
                position = Position.Absolute,
                left = 50,
                bottom = 25,
                fontSize = 24
            }
        } );

    }

    private GridData CreateNewGridData()
    {

        EditorInputPopupWindow.PromptInput( "Create New GridData Asset", "Please define GridData properties:", position,
            out string assetName, out int rows, out int cols, out int cellSize );

        if ( string.IsNullOrEmpty( assetName ) )
            assetName = $"New GridData {_allGridDataObjs.Count + 1}";

        var gridData = CreateInstance<GridData>();

        gridData.name = assetName;
        gridData.CreateGrid( rows, cols, cellSize );

        AssetDatabase.CreateAsset( gridData, GRIDDATA_FOLDER_PATH + assetName + ".asset" );

        return gridData;

    }

    private void OnGridDataSelectionChange( IEnumerable<object> selectedItems )
    {

        var enumerator = selectedItems.GetEnumerator();

        if ( enumerator.MoveNext() )
        {

            var selected = enumerator.Current as GridData;

            if ( enumerator.Current != null )
            {

                _inspectorPanel = new InspectorElement( selected );

                DrawRightView();

            }

        }

    }

    [OnOpenAsset()]
    public static bool OpenGridDataEditor( int instanceID )
    {

        GridData gridData = EditorUtility.InstanceIDToObject( instanceID ) as GridData;

        if ( gridData != null )
        {

            OpenGridDataEditor( gridData );
            return true;

        }

        return false;

    }

    public void OnDestroy()
    {

        var gridDataEditor = FindFirstObjectByType( typeof( GridDataEditor ) ) as GridDataEditor;

        if ( gridDataEditor != null )
        {

            gridDataEditor.Repaint();

        }

    }

    public static void OpenGridDataEditor( GridData gridData )
    {

        GridDataEditorWindow window = GetWindow<GridDataEditorWindow>( "Grid Data Editor" );

        _gridData = gridData;

        window.minSize = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );

        window.Center();

    }

    [MenuItem( "Tools/Grid Data Editor" )]
    public static void OpenGridDataEditor()
    {

        GridDataEditorWindow window = GetWindow<GridDataEditorWindow>( "Grid Data Editor" );

        _gridData = null;

        window.minSize = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );

        window.Center();

    }

}

public class EditorInputPopupWindow : EditorWindow
{

    public static void PromptInput( string title, string desc, Rect parentRect, out string assetName, out int rows, out int cols, out int cellSize, string placeholderInputText = "Name your new GridData", string confirmButtonText = "Confirm", string cancelButtonText = "Cancel" )
    {

        string userInput = string.Empty;
        int inputRows = 0, inputCols = 0, inputCellSize = 0;

        EditorInputPopupWindow window = GetWindow<EditorInputPopupWindow>( title );

        window.rootVisualElement.Add( new Label( desc ) { style = { fontSize = 24 } } );

        var gridInfoContainer = new VisualElement();
        gridInfoContainer.style.flexGrow = 1;
        gridInfoContainer.style.alignContent = Align.Stretch;
        gridInfoContainer.style.alignSelf = Align.Stretch;
        window.rootVisualElement.Add( gridInfoContainer );

        var rowsInputField = new SliderInt( "Rows", 1, 25 )
        {
            value = 15,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( rowsInputField );

        var colsInputField = new SliderInt( "Columns", 1, 25 )
        {
            value = 15,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( colsInputField );

        var cellSizeInputField = new SliderInt( "Cell Size", 20, 60 )
        {
            value = 45,
            showInputField = true,
            style =
            {
                height = 25,
                alignContent = Align.Stretch
            }
        };
        gridInfoContainer.Add( cellSizeInputField );

        var nameInputField = new TextField( "Asset Name" )
        {
            textEdition =
            {
                placeholder = placeholderInputText,
                hidePlaceholderOnFocus = true
            },
            style =
            {
                height = 25
            }

        };
        gridInfoContainer.Add( nameInputField );

        var buttonContainer = new VisualElement();
        gridInfoContainer.Add( buttonContainer );
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.alignSelf = Align.Stretch;
        buttonContainer.style.justifyContent = Justify.SpaceAround;

        var confirmButton = new Button( () =>
        {
            userInput = nameInputField.value;
            inputRows = rowsInputField.value;
            inputCols = colsInputField.value;
            inputCellSize = cellSizeInputField.value;
            window.Close();
        } )
        {
            text = confirmButtonText,
            style =
            {
                height = 40,
                width = StyleKeyword.Auto,
                flexGrow = 1,
                flexDirection = FlexDirection.Column
            }
        };
        buttonContainer.Add( confirmButton );

        var cancelButton = new Button( () =>
        {
            userInput = string.Empty;
            inputRows = 0;
            inputCols = 0;
            inputCellSize = 0;
            window.Close();
        } )
        {
            text = cancelButtonText,
            style =
            {
                height = 40,
                width = StyleKeyword.Auto,
                flexGrow = 1,
                flexDirection = FlexDirection.Column
            }
        };
        buttonContainer.Add( cancelButton );

        window.rootVisualElement.RegisterCallback<KeyUpEvent>( evt =>
        {

            if ( evt.keyCode == KeyCode.Escape )
            {

                userInput = string.Empty;
                inputRows = 0;
                inputCols = 0;
                inputCellSize = 0;

                window.Close();

            }

            if ( evt.keyCode == KeyCode.Return )
            {

                userInput = nameInputField.value;
                inputRows = rowsInputField.value;
                inputCols = colsInputField.value;
                inputCellSize = cellSizeInputField.value;

                window.Close();

            }

        } );

        window.position = new Rect( 0, 0, parentRect.width * 0.20f, parentRect.height * 0.125f );

        window.minSize = new( parentRect.width * 0.4f, parentRect.height * 0.25f );
        window.maxSize = window.minSize;

        window.Center();

        window.ShowModalUtility();

        assetName = userInput;
        rows = inputRows;
        cols = inputCols;
        cellSize = inputCellSize;

    }

}

[CustomEditor( typeof( GridData ) )]
public class GridDataEditor : Editor
{

    private VisualElement _contentContainer, _inspectorContainer, _gridCellsContainer;

    private SerializedProperty _gridProperty, _rowsProperty, _columnsProperty, _cellSizeProperty, _cellStateProperty;

    public override VisualElement CreateInspectorGUI()
    {

        DrawEditor();

        return _inspectorContainer;

    }

    //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
    private static T FindParent<T>( VisualElement element, string name = null ) where T : VisualElement
    {

        VisualElement parent = element;

        do
        {

            parent = parent.parent;

            if ( parent != null && parent.GetType() == typeof( T ) )
            {

                if ( !string.IsNullOrEmpty( name ) && parent.name != name )
                    continue;

                return (T)parent;

            }

        } while ( parent != null );

        return null;

    }
    //End Sourced Code

    private void DrawGrid( bool resizedGrid )
    {

        if ( resizedGrid )
        {

            _gridProperty.ClearArray();

            for ( int r = 0; r < _rowsProperty.intValue; r++ )
            {

                for ( int c = 0; c < _columnsProperty.intValue; c++ )
                {

                    _gridProperty.InsertArrayElementAtIndex( _gridProperty.arraySize );

                    var cell = _gridProperty.GetArrayElementAtIndex( _gridProperty.arraySize - 1 );

                    cell.FindPropertyRelative( "_x" ).intValue = c;
                    cell.FindPropertyRelative( "_y" ).intValue = r;
                    cell.FindPropertyRelative( "_size" ).intValue = _cellSizeProperty.intValue;
                    cell.FindPropertyRelative( "_cellState" ).enumValueIndex = 0;

                }

            }

        }

        serializedObject.ApplyModifiedProperties();

        for ( int i = 0; i < _gridProperty.arraySize; i++ )
        {

            _gridCellsContainer.Add( new PropertyField( _gridProperty.GetArrayElementAtIndex( i ) ) );

        }

    }

    private void DrawLegend( VisualElement gridDataLegendContainer )
    {

        var blackColorLegend = new VisualElement()
        {

            style =
                {

                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.SpaceBetween,
                    height = _cellSizeProperty.intValue,
                    marginBottom = _cellSizeProperty.intValue / 2

                }

        };
        gridDataLegendContainer.Add( blackColorLegend );

        var blackColorBlock = new VisualElement()
        {

            style =
                {

                    height = _cellSizeProperty.intValue,
                    width = _cellSizeProperty.intValue,
                    backgroundColor = Color.black

                }

        };
        blackColorLegend.Add( blackColorBlock );

        blackColorLegend.Add( new Label( "Empty" ) { style = { fontSize = 24, marginLeft = 10, alignSelf = Align.Center } } );

        var redColorLegend = new VisualElement()
        {

            style =
                {

                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.NoWrap,
                    alignItems = Align.FlexStart,
                    justifyContent = Justify.SpaceBetween,
                    height = _cellSizeProperty.intValue

                }

        };
        gridDataLegendContainer.Add( redColorLegend );

        var redColorBlock = new VisualElement()
        {

            style =
                {

                    height = _cellSizeProperty.intValue,
                    width = _cellSizeProperty.intValue,
                    backgroundColor = Color.red

                }

        };
        redColorLegend.Add( redColorBlock );

        redColorLegend.Add( new Label( "Enemy Path" ) { style = { fontSize = 24, marginLeft = 10, alignSelf = Align.Center } } );

    }

    private void DrawInfoContainer( VisualElement gridInfoContainer )
    {

        var rowsInputField = new SliderInt( "Rows", 15, 25 )
        {
            value = _rowsProperty.intValue,
            showInputField = true,
            style =
                {
                    height = 25,
                    flexGrow = 1
                }
        };
        rowsInputField.BindProperty( _rowsProperty );
        rowsInputField.RegisterValueChangedCallback( ( evt ) => { if ( evt.newValue != evt.previousValue ) { DrawGrid( true ); } } );
        gridInfoContainer.Add( rowsInputField );

        var colsInputField = new SliderInt( "Columns", 15, 25 )
        {
            value = _columnsProperty.intValue,
            showInputField = true,
            style =
                {
                    height = 25,
                    flexGrow = 1
                }
        };
        colsInputField.BindProperty( _columnsProperty );
        colsInputField.RegisterValueChangedCallback( ( evt ) => { if ( evt.newValue != evt.previousValue ) { DrawGrid( true ); } } );
        gridInfoContainer.Add( colsInputField );

        var cellSizeInputField = new SliderInt( "Cell Size", 20, 60 )
        {
            value = _cellSizeProperty.intValue,
            showInputField = true,
            style =
                {
                    height = 25,
                    flexGrow = 1
                }
        };
        cellSizeInputField.BindProperty( _cellSizeProperty );
        cellSizeInputField.RegisterValueChangedCallback( ( evt ) => { if ( evt.newValue != evt.previousValue ) { DrawGrid( true ); } } );
        gridInfoContainer.Add( cellSizeInputField );

    }

    private void DrawOpenEditorButton()
    {

        var button = new Button
        {
            text = "Open Editor",
            style =
                {
                    fontSize = 48,
                    flexGrow = 1,
                    flexShrink = 1,
                    flexBasis = StyleKeyword.Auto
                }
        };
        _inspectorContainer.Add( button );

        button.clicked += () => GridDataEditorWindow.OpenGridDataEditor( (GridData)target );

        //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
        _inspectorContainer.RegisterCallback<GeometryChangedEvent>( ( evt ) =>
        {

            if ( _contentContainer == null )
            {

                var rootVisualContainer = FindParent<TemplateContainer>( _inspectorContainer );

                if ( rootVisualContainer != null )
                    _contentContainer = rootVisualContainer.Query<VisualElement>().Where( container => container.name == "unity-content-container" ).AtIndex( 1 );

            }

            if ( _contentContainer != null )
            {

                _contentContainer.parent.style.flexGrow = 1;
                _contentContainer.style.flexGrow = 1;
                _contentContainer.style.height = StyleKeyword.Auto;
                var editorList = _contentContainer.Q<VisualElement>( className: "unity-inspector-editors-list" );
                editorList.style.flexGrow = 1;
                editorList.Children().ElementAt( 1 ).style.flexGrow = 1;
                SetBoxModelValuesToSame( _contentContainer.Q<InspectorElement>(), 10, 0, 5 );
                button.StretchToParentSize();


            }

        } );
        //End Sourced Code

    }

    private void DrawEditor()
    {

        _inspectorContainer = new VisualElement()
        {
            name = "Base-Custom-Element",
            style =
            {

                flexGrow = 1,
                flexDirection = FlexDirection.Row,
                alignItems = Align.FlexStart,
                justifyContent = Justify.SpaceBetween

            }
        };

        if ( !EditorWindow.HasOpenInstances<GridDataEditorWindow>() )
        {

            DrawOpenEditorButton();

            return;

        }

        _gridProperty = serializedObject.FindProperty( "_grid" );
        _rowsProperty = serializedObject.FindProperty( "_rows" );
        _columnsProperty = serializedObject.FindProperty( "_columns" );
        _cellSizeProperty = serializedObject.FindProperty( "_cellSize" );

        var gridDataLegendContainer = new VisualElement()
        {

            style =
            {

                flexGrow = 0,
                flexShrink = 0,
                flexDirection = FlexDirection.Column,
                alignItems = Align.FlexStart,
                justifyContent = Justify.SpaceBetween,
                width = Length.Auto(),
                height = Length.Auto(),
                marginTop = 10

            }

        };
        _inspectorContainer.Add( gridDataLegendContainer );
        DrawLegend( gridDataLegendContainer );

        var inspector = new VisualElement()
        {

            style =
            {

                flexGrow = 1,
                flexDirection = FlexDirection.Column,
                alignItems = Align.Center,
                alignSelf = Align.Center,
                justifyContent = Justify.SpaceBetween,
                paddingTop = 20

            }

        };
        _inspectorContainer.Add( inspector );

        _gridCellsContainer = new VisualElement()
        {

            style =
                {

                    width = _cellSizeProperty.intValue * _columnsProperty.intValue,
                    height = _cellSizeProperty.intValue * _rowsProperty.intValue,
                    flexDirection = FlexDirection.Row,
                    flexGrow = 0,
                    flexShrink = 1,
                    alignContent = Align.FlexStart,
                    justifyContent = Justify.Center,
                    flexWrap = Wrap.WrapReverse

                }

        };
        inspector.Add( _gridCellsContainer );
        DrawGrid( false );

        var gridInfoContainer = new VisualElement()
        {

            style =
                {

                    paddingTop = 20,
                    flexDirection = FlexDirection.Column,
                    flexGrow = 1,
                    flexShrink = 1,
                    alignContent = Align.Center,
                    width = Length.Percent( 80 ),
                    maxHeight = 100

                }

        };
        inspector.Add( gridInfoContainer );
        DrawInfoContainer( gridInfoContainer );

        serializedObject.ApplyModifiedProperties();

    }

    private void SetBoxModelValuesToSame( VisualElement element, int margin = 0, int border = 0, int padding = 0 )
    {

        element.style.paddingBottom = padding;
        element.style.paddingLeft = padding;
        element.style.paddingRight = padding;
        element.style.paddingTop = padding;
        element.style.marginBottom = margin;
        element.style.marginLeft = margin;
        element.style.marginRight = margin;
        element.style.marginTop = margin;
        element.style.borderBottomWidth = border;
        element.style.borderLeftWidth = border;
        element.style.borderRightWidth = border;
        element.style.borderTopWidth = border;

    }

}

[CustomPropertyDrawer( typeof( Cell ) )]
public class CellDrawer : PropertyDrawer
{

    public override VisualElement CreatePropertyGUI( SerializedProperty property )
    {

        var xProperty = property.FindPropertyRelative( "_x" );
        var yProperty = property.FindPropertyRelative( "_y" );
        var sizeProperty = property.FindPropertyRelative( "_size" );
        var stateProperty = property.FindPropertyRelative( "_cellState" );

        var parentContainer = new VisualElement();

        parentContainer.style.alignItems = Align.Center;

        parentContainer.Add( DrawCell( xProperty, yProperty, sizeProperty, stateProperty, parentContainer ) );

        parentContainer.Add( new Label( $"{xProperty.intValue}, {yProperty.intValue}" ) { style = { position = Position.Absolute, alignSelf = Align.Center, fontSize = sizeProperty.intValue / 2.75f }, pickingMode = PickingMode.Ignore } );

        return parentContainer;

    }

    private VisualElement DrawCell( SerializedProperty xProperty, SerializedProperty yProperty, SerializedProperty sizeProperty, SerializedProperty stateProperty, VisualElement parentContainer )
    {

        var element = new VisualElement()
        {
            name = $"Cell {xProperty.intValue}, {yProperty.intValue}",
            style =
            {
                width = sizeProperty.intValue,
                height = sizeProperty.intValue,
                borderBottomWidth = 1,
                borderTopWidth = 1,
                borderLeftWidth = 1,
                borderRightWidth = 1,
                borderBottomColor = Color.white,
                borderTopColor = Color.white,
                borderLeftColor = Color.white,
                borderRightColor = Color.white,
                backgroundColor = stateProperty.enumValueIndex == 0 ? Color.black : Color.red
            }

        };

        parentContainer.RegisterCallback<ClickEvent>( ( evt ) => { ToggleCellStatus( stateProperty, element ); } );

        return element;


    }

    private void ToggleCellStatus( SerializedProperty stateProperty, VisualElement element )
    {

        if ( stateProperty.enumValueIndex == (int)Cell.CellState.Empty )
        {

            stateProperty.enumValueIndex = (int)Cell.CellState.Path;

        }
        else if ( stateProperty.enumValueIndex == (int)Cell.CellState.Path )
        {

            stateProperty.enumValueIndex = (int)Cell.CellState.Empty;

        }

        stateProperty.serializedObject.ApplyModifiedProperties();

        element.style.backgroundColor = stateProperty.enumValueIndex == 0 ? Color.black : Color.red;

    }

}
