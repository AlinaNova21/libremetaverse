using System.Linq;
using LibreMetaverse.Rendering;
using NUnit.Framework;

namespace LibreMetaverse.Rendering.Tests
{
    /// <summary>
    /// Prim faces are numbered top, sides, bottom, and texture entries are indexed by those numbers,
    /// so a missing cap or an extra face puts textures on the wrong faces.
    /// </summary>
    [TestFixture]
    public class PrimEndCapTests
    {
        private static FacetedMesh Mesh(ProfileCurve profile)
        {
            var prim = new Primitive();
            var data = prim.PrimData;
            data.PCode = PCode.Prim;
            data.ProfileCurve = profile;
            data.PathCurve = PathCurve.Line;
            data.ProfileEnd = 1f;
            data.PathEnd = 1f;
            data.PathScaleX = 1f;
            data.PathScaleY = 1f;
            data.PathRevolutions = 1f;
            prim.PrimData = data;
            return new global::LibreMetaverse.Rendering.MeshFoundry().GenerateFacetedMesh(prim, DetailLevel.High);
        }

        private static Vector3 SurfaceDirection(Face face)
        {
            var sum = Vector3.Zero;
            for (var i = 0; i + 2 < face.Indices.Count; i += 3)
            {
                var a = face.Vertices[face.Indices[i]].Position;
                var b = face.Vertices[face.Indices[i + 1]].Position;
                var c = face.Vertices[face.Indices[i + 2]].Position;
                sum += (b - a) % (c - a);
            }
            return sum;
        }

        [TestCase(ProfileCurve.Square, 6)]
        [TestCase(ProfileCurve.Circle, 3)]
        [TestCase(ProfileCurve.EqualTriangle, 5)]
        public void Solid_prims_have_the_expected_number_of_faces(ProfileCurve profile, int faces)
        {
            Assert.That(Mesh(profile).Faces, Has.Count.EqualTo(faces));
        }

        [TestCase(ProfileCurve.Square)]
        [TestCase(ProfileCurve.Circle)]
        [TestCase(ProfileCurve.EqualTriangle)]
        public void Bottom_face_is_last_and_faces_down(ProfileCurve profile)
        {
            var bottom = Mesh(profile).Faces.Last();

            Assert.That(bottom.Vertices.All(v => v.Position.Z < -0.49f), Is.True);
            Assert.That(bottom.Vertices.All(v => v.Normal.Z < -0.99f), Is.True);
            Assert.That(SurfaceDirection(bottom).Z, Is.LessThan(0f), "the winding must face outward");
        }

        [Test]
        public void Box_has_no_face_without_area()
        {
            Assert.That(Mesh(ProfileCurve.Square).Faces.Select(f => SurfaceDirection(f).Length()), Has.All.GreaterThan(0.5f));
        }

        private static FacetedMesh CutBox(float begin, float end)
        {
            var prim = new Primitive();
            var data = prim.PrimData;
            data.PCode = PCode.Prim;
            data.ProfileCurve = ProfileCurve.Square;
            data.PathCurve = PathCurve.Line;
            data.ProfileBegin = begin;
            data.ProfileEnd = end;
            data.PathEnd = 1f;
            data.PathScaleX = 1f;
            data.PathScaleY = 1f;
            data.PathRevolutions = 1f;
            prim.PrimData = data;
            return new global::LibreMetaverse.Rendering.MeshFoundry().GenerateFacetedMesh(prim, DetailLevel.High);
        }

        // Face numbers run over the faces that exist. Texture entries are indexed by them, so a
        // skipped number puts the texture for one face on the next one.
        [TestCase(0.5f, 1f, 6, 3)]
        [TestCase(0f, 0.5f, 6, 3)]
        [TestCase(0.125f, 0.625f, 7, 4)]
        [TestCase(0.25f, 1f, 7, 4)]
        [TestCase(0.25f, 0.75f, 6, 3)]
        public void Cut_boxes_have_no_gaps_in_their_face_numbers(float begin, float end, int faces, int bottom)
        {
            var mesh = CutBox(begin, end);

            Assert.That(mesh.Faces, Has.Count.EqualTo(faces));
            Assert.That(mesh.Faces.Select(f => f.Vertices.Count), Has.All.GreaterThan(0), "no face number may be empty");
            Assert.That(mesh.Faces[bottom].Vertices.All(v => v.Position.Z < -0.49f), Is.True, "the bottom follows the outer sides");
        }
    }
}
