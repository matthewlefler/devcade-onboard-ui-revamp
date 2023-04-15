using System.Collections.Generic;

namespace onboard.devcade;

/// <summary>
/// A game on Devcade. Contains information about the game, such as the author, description, tags, and more.
/// </summary>
public class DevcadeGame {
    /// <summary>
    /// The author's username, or the author's google username if the author is not a CSH member.
    /// </summary>
    public string author { get; set; }
    
    /// <summary>
    /// The description of the game, as provided by the author.
    /// </summary>
    public string description { get; set; }
    
    /// <summary>
    /// The hash of the game, used to verify the integrity of the game, and to determine whether the
    /// game has been updated.
    /// </summary>
    public string hash { get; set; }
    
    /// <summary>
    /// The game's ID, used to identify the game. This will not change even if the game is updated.
    /// </summary>
    public string id { get; set; }
    
    /// <summary>
    /// The name of the game, as provided by the author.
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// The tags associated with the game, used to categorize and filter games.
    /// </summary>
    public List<Tag> tags { get; set; }

    /// <summary>
    /// The date the game was uploaded, in the format YYYY-MM-DD.
    /// </summary>
    public string upload_date { get; set; }
    
    /// <summary>
    /// The user that uploaded the game.
    /// </summary>
    public User user { get; set; }
    
    public DevcadeGame(string author, string description, string hash, string id, string name, List<Tag> tags, string upload_date, User user) {
        this.author = author;
        this.description = description;
        this.hash = hash;
        this.id = id;
        this.name = name;
        this.tags = tags;
        this.upload_date = upload_date;
        this.user = user;
    }
    
    public DevcadeGame() {
        this.author = "";
        this.description = "";
        this.hash = "";
        this.id = "";
        this.name = "";
        this.tags = new List<Tag>();
        this.upload_date = "";
        this.user = new User();
    }
}

/// <summary>
/// A tag from the Devcade API that is associated with a game. Used to categorize games.
/// </summary>
public class Tag {
    /// <summary>
    /// The tag's name, which uniquely identifies a tag.
    /// </summary>
    public string name { get; set; }
    
    /// <summary>
    /// The tag's description, used to describe the tag.
    /// </summary>
    public string description { get; set; }
    
    public Tag(string name, string description) {
        this.name = name;
        this.description = description;
    }

    public Tag() {
        this.name = "";
        this.description = "";
    }
}

/// <summary>
/// A user from the Devcade API that is associated with a game. Used to identify the author od a game.
/// The user type is used to detemine whether the user is a CSH member or a Google user.
/// </summary>
public class User {
    /// <summary>
    /// Whether the user is an admin.
    /// </summary>
    public bool admin { get; set; }
    
    /// <summary>
    /// The user's email address.
    /// </summary>
    public string email { get; set; }
    
    /// <summary>
    /// The user's first name.
    /// </summary>
    public string first_name { get; set; }
    
    /// <summary>
    /// The user's ID, used to uniquely identify the user.
    /// </summary>
    public string id { get; set; }
    
    /// <summary>
    /// The user's last name.
    /// </summary>
    public string last_name { get; set; }
    
    /// <summary>
    /// a URL to the uer's profile picture.
    /// </summary>
    public string picture { get; set; }
    
    /// <summary>
    /// The user's type, currently either CSH or GOOGLE.
    /// </summary>
    public UserType user_type { get; set; }
    
    public User(bool admin, string email, string first_name, string id, string last_name, string picture, UserType user_type) {
        this.admin = admin;
        this.email = email;
        this.first_name = first_name;
        this.id = id;
        this.last_name = last_name;
        this.picture = picture;
        this.user_type = user_type;
    }
    
    public User() {
        this.admin = false;
        this.email = "";
        this.first_name = "";
        this.id = "";
        this.last_name = "";
        this.picture = "";
        this.user_type = UserType.GOOGLE;
    }
}

/// <summary>
/// The type of the user. This is used to determine whether the user is a CSH member or a Google user.
/// </summary>
public enum UserType {
    /// <summary>
    /// A CSH member. Games made by CSH members can use the Gatekeeper API to authenticate other
    /// CSH members.
    /// </summary>
    CSH,
    
    /// <summary>
    /// A Google user. This user is not associated with CSH, and cannot use the Gatekeeper API.
    /// </summary>
    GOOGLE,
}