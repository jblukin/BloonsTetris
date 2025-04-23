using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GridDataEditorWindow : EditorWindow
{

    private const string GRIDDATA_FOLDER_PATH = "Assets/_Scripts/Grid/GridData Objs/";

    private VisualElement _gridPanel, _inspectorPanel;

    private void CreateGUI()
    {

        var GridDataGUIDs = AssetDatabase.FindAssets( "t:GridData" );

        List<GridData> allGridDataObjs = new();

        foreach ( var guid in GridDataGUIDs )
            allGridDataObjs.Add( AssetDatabase.LoadAssetAtPath<GridData>( AssetDatabase.GUIDToAssetPath( guid ) ) );

        var splitView = new TwoPaneSplitView( 0, 240f, TwoPaneSplitViewOrientation.Horizontal );
        rootVisualElement.Add( splitView );

        var leftView = new ListView
        {
            makeItem = () => new Label(),
            bindItem = ( item, idx ) => { ( item as Label ).text = allGridDataObjs[ idx ].name; },
            itemsSource = allGridDataObjs,
            selectionType = SelectionType.Single
        };
        splitView.Add( leftView );

        leftView.selectionChanged += OnGridDataSelectionChange;

        leftView.AddManipulator( new ContextualMenuManipulator( ( evt ) =>
        {

            evt.menu.AppendAction( "Create New GridData Asset", action => CreateNewGridData() );

        } ) );

        _inspectorPanel = new ScrollView( ScrollViewMode.VerticalAndHorizontal );
        splitView.Add( _inspectorPanel );

    }

    private void CreateNewGridData()
    {

        string assetName = EditorInputPopupWindow.PromptInput( "Create New GridData Asset", "Please define GridData properties:", position );

        if ( string.IsNullOrEmpty( assetName ) )
            return;

        var gridData = CreateInstance<GridData>();

        AssetDatabase.CreateAsset( gridData, GRIDDATA_FOLDER_PATH + assetName + ".asset" );

    }

    private void OnGridDataSelectionChange( IEnumerable<object> selectedItems )
    {

        _inspectorPanel.Clear();

        var enumerator = selectedItems.GetEnumerator();

        if ( enumerator.MoveNext() )
        {

            var selected = enumerator.Current as GridData;

            if ( enumerator.Current != null )
            {

                var inspector = new InspectorElement( selected );
                _inspectorPanel.Add( inspector );

            }

        }

    }

    [MenuItem( "Tools/Grid Data Editor" )]
    public static void OpenGridDataEditor()
    {

        GridDataEditorWindow window = GetWindow<GridDataEditorWindow>( "Grid Data Editor" );

        window.minSize = new Vector2( Screen.currentResolution.width / 2, Screen.currentResolution.height / 2 );
        window.maxSize = new Vector2( Screen.currentResolution.width / 1.5f, Screen.currentResolution.height / 1.5f );

        window.position = new Rect( window.minSize - window.position.size / 2, window.maxSize );

    }

}

public class EditorInputPopupWindow : EditorWindow
{

    public static string PromptInput( string title, string desc, Rect parentRect, string placeholderInputText = "Name your new GridData", string confirmButtonText = "Confirm", string cancelButtonText = "Cancel" )
    {

        string userInput = string.Empty;

        EditorInputPopupWindow window = GetWindow<EditorInputPopupWindow>( title );

        window.rootVisualElement.Add( new Label( desc ) );

        var gridInfoContainer = new VisualElement();
        window.rootVisualElement.Add( gridInfoContainer );

        var rowsInputField = new IntegerField( "Rows" );
        gridInfoContainer.Add( rowsInputField );

        var colsInputField = new IntegerField( "Columns" );
        gridInfoContainer.Add( colsInputField );

        var cellSizeInputField = new IntegerField( "Cell Size" );
        gridInfoContainer.Add( cellSizeInputField );

        var nameInputField = new TextField( "Asset Name" )
        {
            textEdition =
            {
                placeholder = placeholderInputText,
                hidePlaceholderOnFocus = true
            }

        };
        window.rootVisualElement.Add( nameInputField );

        var buttonContainer = new VisualElement();
        window.rootVisualElement.Add( buttonContainer );
        buttonContainer.style.flexDirection = FlexDirection.Row;
        buttonContainer.style.justifyContent = Justify.SpaceAround;

        var confirmButton = new Button( () => { userInput = nameInputField.value; window.Close(); } )
        {
            text = confirmButtonText
        };
        buttonContainer.Add( confirmButton );
        confirmButton.style.flexGrow = 1;

        var cancelButton = new Button( () => { userInput = string.Empty; window.Close(); } )
        {
            text = cancelButtonText
        };
        buttonContainer.Add( cancelButton );
        cancelButton.style.flexGrow = 1;

        window.rootVisualElement.RegisterCallback<KeyUpEvent>( evt =>
        {

            if ( evt.keyCode == KeyCode.Escape )
            {

                userInput = string.Empty;

                window.Close();

            }

            if ( evt.keyCode == KeyCode.Return )
            {

                userInput = nameInputField.value;

                window.Close();

            }

        } );

        window.position = new Rect( parentRect.x, parentRect.y, parentRect.width / 4, parentRect.height / 4 );

        //window.rootVisualElement.schedule.Execute( () =>
        //{

        //    nameInputField.Focus();

        //} ).ExecuteLater( 10 );

        window.ShowModalUtility();

        return userInput;

    }

}

[CustomEditor( typeof( GridData ) )]
public class GridDataEditor : Editor
{

    private VisualElement contentContainer;

    public override VisualElement CreateInspectorGUI()
    {

        var inspector = new VisualElement()
        {
            name = "Base-Custom-Element"
        };

        if ( EditorWindow.HasOpenInstances<GridDataEditorWindow>() )
            InspectorElement.FillDefaultInspector( inspector, serializedObject, this );

        else
        {
            //Original Code Sourced Here: https://discussions.unity.com/t/make-an-editor-expand-to-the-inspector-size/896693
            inspector.RegisterCallback<GeometryChangedEvent, VisualElement>( ( evt, args ) =>
            {

                if ( contentContainer == null )
                {

                    var rootVisualContainer = FindParent<TemplateContainer>( args );

                    if ( rootVisualContainer != null )
                        contentContainer = rootVisualContainer.Query<VisualElement>().Where( container => container.parent.name == "unity-content-viewport" ).First();

                }

                if ( contentContainer != null )
                {

                    Debug.Log( "Found" );

                    contentContainer.style.maxWidth = contentContainer.parent.resolvedStyle.width;
                    contentContainer.style.flexGrow = 1;
                    args.style.height = contentContainer.resolvedStyle.height - 90;
                    SetBoxModelValuesToSame( contentContainer.Q<VisualElement>( className: "unity-inspector-element" ), 0, 0, 0 );

                }

            }, inspector );
            //End Sourced Code

            var button = new Button
            {
                text = "Open Editor",
                style =
                {
                    fontSize = 48
                }
            };
            inspector.Add( button );

            button.StretchToParentSize();

            button.clicked += () => GridDataEditorWindow.OpenGridDataEditor();

        }

        return inspector;

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

public class GridDataHandler
{

    [OnOpenAsset()]
    public static bool OpenGridDataEditor( int instanceID )
    {

        GridData gridData = EditorUtility.InstanceIDToObject( instanceID ) as GridData;

        if ( gridData != null )
        {

            GridDataEditorWindow.OpenGridDataEditor();
            return true;

        }

        return false;

    }

}
