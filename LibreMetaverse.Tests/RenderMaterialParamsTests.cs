using System;
using NUnit.Framework;

namespace LibreMetaverse.Tests
{
    [TestFixture]
    [Category("Primitives")]
    public class RenderMaterialParamsTests
    {
        private const ushort RenderMaterialType = 0x80;

        private static byte[] Block(params (byte Face, UUID Id)[] materials)
        {
            var length = 1 + materials.Length * 17;
            var data = new byte[1 + 2 + 4 + length];
            data[0] = 1;
            Utils.UInt16ToBytes(RenderMaterialType).CopyTo(data, 1);
            Utils.UIntToBytes((uint)length).CopyTo(data, 3);
            data[7] = (byte)materials.Length;
            var at = 8;
            foreach (var (face, id) in materials)
            {
                data[at] = face;
                id.ToBytes(data, at + 1);
                at += 17;
            }
            return data;
        }

        [Test]
        public void Materials_are_read_by_face()
        {
            var (first, second) = (UUID.Random(), UUID.Random());
            var prim = new Primitive();

            prim.SetExtraParamsFromBytes(Block((0, first), (3, second)), 0);

            Assert.That(prim.RenderMaterials[0], Is.EqualTo(first));
            Assert.That(prim.RenderMaterials[3], Is.EqualTo(second));
        }

        [Test]
        public void Length_read_includes_the_block()
        {
            var data = Block((0, UUID.Random()));

            Assert.That(new Primitive().SetExtraParamsFromBytes(data, 0), Is.EqualTo(data.Length));
        }

        [Test]
        public void Truncated_block_keeps_only_whole_entries()
        {
            var data = Block((0, UUID.Random()), (1, UUID.Random()));
            Utils.UIntToBytes(1 + 17 + 5).CopyTo(data, 3);
            var prim = new Primitive();

            prim.SetExtraParamsFromBytes(data, 0);

            Assert.That(prim.RenderMaterials, Has.Count.EqualTo(1));
        }
    }
}
