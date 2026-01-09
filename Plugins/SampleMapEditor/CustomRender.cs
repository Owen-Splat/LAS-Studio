using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace SampleMapEditor
{
    /// <summary>
    /// Represents a custom renderer that can be transformed and manipulated.
    /// </summary>
    public class CustomRender : EditableObject, IColorPickable
    {
        public static Vector4 AnimatedColor = new Vector4(1);

        UVCubeRenderer CubeDrawer;
        StandardMaterial Material;

        public CustomRender(NodeBase parent = null) : base(parent)
        {
            //Prepare our renderable sphere
            CubeDrawer = new UVCubeRenderer(0.5f);
            //The gl framework includes some base materials to easily use
            Material = new StandardMaterial();
            //We can also apply some in engine textures
            Material.DiffuseTextureID = RenderTools.TexturedCubeTex.ID;
        }

        public void DrawColorPicking(GLContext context)
        {
            //Here we can draw under a color picking shader
            CubeDrawer.DrawPicking(context, this, Transform.TransformMatrix);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            //Make sure to draw on the right pass!
            //These are used to sort out transparent ordering
            if (pass == Pass.OPAQUE)
                DrawOpaque(context);
        }

        private void DrawOpaque(GLContext context)
        {
            //Apply material
            Material.ModelMatrix = this.Transform.TransformMatrix;
            Material.Color = AnimatedColor;
            //Disable textures for animated colors if used
            if (AnimatedColor != OpenTK.Vector4.One)
                Material.DiffuseTextureID = -1;

            Material.Render(context);
            //Draw with a selection visual. 
            CubeDrawer.DrawWithSelection(context, this.IsSelected || this.IsHovered);
        }
    }


    /// <summary>
    /// Vertex structure for lighting rendering.
    /// </summary>
    struct LightVertex
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 12)]
        public Vector4 Color;

        public LightVertex(Vector3 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }

    /// <summary>
    /// Represents a sphere renderer that can be transformed and manipulated
    /// </summary>
    public class LightRender : EditableObject, IColorPickable
    {
        private RenderMesh<LightVertex> AxisRenderer;

        public static Vector4 AnimatedColor = new Vector4(1.0f, 0.8f, 0.2f, 0.5f);

        UVSphereRender LightDrawer;
        StandardMaterial Material;

        public LightRender(NodeBase parent = null) : base(parent)
        {
            //Prepare our renderable sphere
            LightDrawer = new UVSphereRender(0.5f);
            // The gl framework includes some base materials to easily use
            Material = new StandardMaterial();
            // Create axis lines to show direction
            AxisRenderer = new RenderMesh<LightVertex>(GetAxisVertices(), PrimitiveType.Lines);
        }

        public void DrawColorPicking(GLContext context)
        {
            //Here we can draw under a color picking shader
            LightDrawer.DrawPicking(context, this, Transform.TransformMatrix);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            //Make sure to draw on the right pass!
            //These are used to sort out transparent ordering
            if (pass == Pass.OPAQUE)
                DrawOpaque(context);
        }

        private void DrawOpaque(GLContext context)
        {
            //Apply material
            Material.ModelMatrix = this.Transform.TransformMatrix;
            Material.Color = AnimatedColor;
            //Disable textures for animated colors if used
            if (AnimatedColor != Vector4.One)
                Material.DiffuseTextureID = -1;

            Material.Render(context);
            //Draw with a selection visual. 
            LightDrawer.DrawWithSelection(context, this.IsSelected || this.IsHovered);

            // draw axis
            AxisRenderer.Draw(context);
        }

        /// <summary>
        /// Creates axis indicator lines at the center.
        /// Shows X (Red), Y (Green), Z (Blue) directions.
        /// </summary>
        private static LightVertex[] GetAxisVertices()
        {
            float len = 1.75f; // Axis length (10% of cube size)

            Vector4 colorX = new Vector4(1.0f, 0.0f, 0.0f, 1.0f); // Red = X = Right
            Vector4 colorY = new Vector4(0.0f, 1.0f, 0.0f, 1.0f); // Green = Y = Up
            Vector4 colorZ = new Vector4(0.0f, 0.5f, 1.0f, 1.0f); // Blue = Z = Front

            return new LightVertex[]
            {
                // // X axis (Red) - Right direction
                // new LightVertex(Vector3.Zero, colorX),
                // new LightVertex(new Vector3(len, 0, 0), colorX),
                // // Arrow head for X
                // new LightVertex(new Vector3(len, 0, 0), colorX),
                // new LightVertex(new Vector3(len * 0.7f, len * 0.15f, 0), colorX),
                // new LightVertex(new Vector3(len, 0, 0), colorX),
                // new LightVertex(new Vector3(len * 0.7f, -len * 0.15f, 0), colorX),

                // // Y axis (Green) - Up direction
                // new LightVertex(Vector3.Zero, colorY),
                // new LightVertex(new Vector3(0, len, 0), colorY),
                // // Arrow head for Y
                // new LightVertex(new Vector3(0, len, 0), colorY),
                // new LightVertex(new Vector3(len * 0.15f, len * 0.7f, 0), colorY),
                // new LightVertex(new Vector3(0, len, 0), colorY),
                // new LightVertex(new Vector3(-len * 0.15f, len * 0.7f, 0), colorY),

                // Z axis (Blue) - Front direction
                new LightVertex(Vector3.Zero, colorZ),
                new LightVertex(new Vector3(0, 0, len), colorZ),
                // Arrow head for Z
                new LightVertex(new Vector3(0, 0, len), colorZ),
                new LightVertex(new Vector3(len * 0.15f, 0, len * 0.7f), colorZ),
                new LightVertex(new Vector3(0, 0, len), colorZ),
                new LightVertex(new Vector3(-len * 0.15f, 0, len * 0.7f), colorZ),
            };
        }

        public override void Dispose()
        {
            AxisRenderer?.Dispose();
        }
    }


    /// <summary>
    /// Vertex structure for colored wireframe rendering.
    /// </summary>
    struct WireframeVertex
    {
        [RenderAttribute(0, VertexAttribPointerType.Float, 0)]
        public Vector3 Position;

        [RenderAttribute(GLConstants.VColor, VertexAttribPointerType.Float, 12)]
        public Vector4 Color;

        public WireframeVertex(Vector3 position, Vector4 color)
        {
            Position = position;
            Color = color;
        }
    }

    /// <summary>
    /// Renders an area as a colored wireframe box
    /// Scales correctly with the object's transform
    /// </summary>
    public class AreaWireframeRender : EditableObject, IColorPickable, IFrustumCulling
    {
        private RenderMesh<WireframeVertex> WireframeRenderer;
        private UVCubeRenderer PickingCubeRenderer; // Solid cube for click detection
        private StandardMaterial Material;

        public bool EnableFrustumCulling => true;
        public bool InFrustum { get; set; }

        public BoundingNode Boundings = new BoundingNode()
        {
            Center = new Vector3(0, 0, 0),
            Box = new BoundingBox(new Vector3(-10f), new Vector3(10f)),
        };

        public bool IsInsideFrustum(GLContext context)
        {
            return context.Camera.InFustrum(Boundings);
        }

        public AreaWireframeRender(NodeBase parent = null) : base(parent)
        {
            // Update boundings on transform changed
            this.Transform.TransformUpdated += delegate {
                Boundings.UpdateTransform(this.Transform.TransformMatrix);
            };

            // Create wireframe cube with unit size (scale will be applied via transform)
            WireframeRenderer = new RenderMesh<WireframeVertex>(GetWireframeVertices(), PrimitiveType.Lines);

            // Create solid cube for color picking (same size as wireframe: half-size 5)
            PickingCubeRenderer = new UVCubeRenderer(0.55f);

            Material = new StandardMaterial();
            Material.hasVertexColors = true;

            UINode.Tag = this;
        }

        public void DrawColorPicking(GLContext context)
        {
            // Use solid cube for picking - much easier to click than wireframe lines
            PickingCubeRenderer.DrawPicking(context, this, Transform.TransformMatrix);
        }

        public override void DrawModel(GLContext context, Pass pass)
        {
            if (pass != Pass.OPAQUE || !this.InFrustum)
                return;

            base.DrawModel(context, pass);

            Material.ModelMatrix = Transform.TransformMatrix;
            Material.Render(context);

            // Draw thicker lines when selected
            float lineWidth = (IsSelected || IsHovered) ? 3.0f : 2.0f;
            GL.LineWidth(lineWidth);

            // Draw wireframe cube
            WireframeRenderer.Draw(context);
            GL.LineWidth(1);
        }

        /// <summary>
        /// Creates wireframe cube vertices with colored edges.
        /// Unit cube from -0.5 to 0.5 (centered at origin)
        /// </summary>
        private static WireframeVertex[] GetWireframeVertices()
        {
            float s = 0.55f; // half-size (same as default cube size 10)

            // Colors for different axes
            Vector4 colorX = new Vector4(1.0f, 0.3f, 0.3f, 1.0f); // Red for X edges
            Vector4 colorY = new Vector4(0.3f, 1.0f, 0.3f, 1.0f); // Green for Y edges
            Vector4 colorZ = new Vector4(0.3f, 0.6f, 1.0f, 1.0f); // Blue for Z edges
            Vector4 colorBase = new Vector4(0.7f, 0.7f, 0.7f, 1.0f); // Gray for base

            // 8 corners of the cube
            Vector3 v0 = new Vector3(-s, -s, -s); // back-bottom-left
            Vector3 v1 = new Vector3( s, -s, -s); // back-bottom-right
            Vector3 v2 = new Vector3( s,  s, -s); // back-top-right
            Vector3 v3 = new Vector3(-s,  s, -s); // back-top-left
            Vector3 v4 = new Vector3(-s, -s,  s); // front-bottom-left
            Vector3 v5 = new Vector3( s, -s,  s); // front-bottom-right
            Vector3 v6 = new Vector3( s,  s,  s); // front-top-right
            Vector3 v7 = new Vector3(-s,  s,  s); // front-top-left

            List<WireframeVertex> vertices = new List<WireframeVertex>
            {
                // Bottom face edges (Y = -s) - use base color
                new WireframeVertex(v0, colorBase),
                new WireframeVertex(v1, colorBase),
                new WireframeVertex(v1, colorBase),
                new WireframeVertex(v5, colorBase),
                new WireframeVertex(v5, colorBase),
                new WireframeVertex(v4, colorBase),
                new WireframeVertex(v4, colorBase),
                new WireframeVertex(v0, colorBase),

                // Top face edges (Y = +s) - use green (Y axis)
                new WireframeVertex(v3, colorY),
                new WireframeVertex(v2, colorY),
                new WireframeVertex(v2, colorY),
                new WireframeVertex(v6, colorY),
                new WireframeVertex(v6, colorY),
                new WireframeVertex(v7, colorY),
                new WireframeVertex(v7, colorY),
                new WireframeVertex(v3, colorY),

                // Vertical edges (connecting bottom to top) - use green (Y axis)
                new WireframeVertex(v0, colorY),
                new WireframeVertex(v3, colorY),
                new WireframeVertex(v1, colorY),
                new WireframeVertex(v2, colorY),
                new WireframeVertex(v5, colorY),
                new WireframeVertex(v6, colorY),
                new WireframeVertex(v4, colorY),
                new WireframeVertex(v7, colorY)
            };

            return vertices.ToArray();
        }

        public override void Dispose()
        {
            WireframeRenderer?.Dispose();
            PickingCubeRenderer?.Dispose();
        }
    }
}
