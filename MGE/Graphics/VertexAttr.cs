// using System.Reflection;
// using OpenTK.Graphics.OpenGL;

// namespace MGE.Graphics
// {
// 	public class VertexAttr
// 	{
// 		public int index { get; private set; }

// 		public int components { get; private set; }

// 		public VertexAttribPointerType Type { get; private set; }

// 		public bool Normalized { get; private set; }

// 		internal VertexAttrib()
// 		{
// 		}

// 		internal override void Initialize(Program program, PropertyInfo property)
// 		{
// 			base.Initialize(program, property);
// 			var attribute = property.GetCustomAttributes<VertexAttribAttribute>(false).FirstOrDefault() ?? new VertexAttribAttribute();
// 			components = attribute.Components;
// 			Type = attribute.Type;
// 			Normalized = attribute.Normalized;
// 			if (attribute.Index > 0) BindAttribLocation(attribute.Index);
// 		}

// 		public void BindAttribLocation(int index)
// 		{
// 			this.index = index;
// 			GL.BindAttribLocation(ProgramHandle, index, Name);
// 		}

// 		internal override void OnLink()
// 		{
// 			index = GL.GetAttribLocation(ProgramHandle, Name);
// 			Active = index > -1;
// 			if (!Active) Logger?.WarnFormat("Vertex attribute not found or not active: {0}", Name);
// 		}
// 	}
// }
