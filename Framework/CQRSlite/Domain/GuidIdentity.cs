using System;

namespace CQRSlite.Domain
{
    public class GuidIdentity : IIdentity, IEquatable<GuidIdentity>, IEquatable<IIdentity>
    {
        private readonly Guid _id;
        
        public string Id => _id.ToString();
        public bool IsValid => _id != Guid.Empty;

        public GuidIdentity(Guid id)
        {
            _id = id;
        }

        public override string ToString() => _id.ToString();

        public override int GetHashCode() => _id.GetHashCode();

        public override bool Equals(object obj) => Equals(obj as GuidIdentity);

        public bool Equals(IIdentity other) => Equals(other as GuidIdentity);

        public bool Equals(GuidIdentity other) => object.ReferenceEquals(other, null) ? false : other.Id == Id;

        public static bool operator ==(GuidIdentity a, GuidIdentity b)
        {
            if (object.ReferenceEquals(a, null))
                return object.ReferenceEquals(b, null);
            return a.Equals(b);
        }

        public static bool operator !=(GuidIdentity a, GuidIdentity b) => !(a == b);

        public static GuidIdentity Create() => new GuidIdentity(Guid.NewGuid());
    }
}