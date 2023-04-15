use serde::{Deserialize, Serialize};

/**
 * A tag from the Devcade API that is associated with a game. Used to categorize games.
 */
#[derive(Default, Debug, Serialize, Deserialize)]
pub struct Tag {
    /**
     * The tag's description, used to describe the tag.
     */
    pub description: String,

    /**
     * The tag's name, which uniquely identifies a tag.
     */
    pub name: String,
}

/**
 * A user from the Devcade API that is associated with a game. Used to identify the author of a game.
 * The user type is used to determine whether the user is a CSH member or a Google user.
 */
#[derive(Default, Debug, Serialize, Deserialize)]
pub struct User {
    /**
     * Whether the user is an admin.
     */
    pub admin: bool,

    /**
     * The user's email address.
     */
    pub email: String,

    /**
     * The user's first name.
     */
    pub first_name: String,

    /**
     * The user's ID, used to uniquely identify the user.
     */
    pub id: String,

    /**
     * The user's last name.
     */
    pub last_name: String,

    /**
     * a URL to the user's profile picture.
     */
    pub picture: String,

    /**
     * The user's type, currently either CSH or GOOGLE.
     */
    pub user_type: UserType,
}

/**
 * The type of user. This is used to determine whether the user is a CSH member or a Google user.
 */
#[derive(Default, Debug, Serialize, Deserialize)]
pub enum UserType {
    /**
     * A CSH member. Games made by CSH members can use the Gatekeeper API to authenticate other
     * CSH members.
     */
    CSH,

    /**
     * A Google user. This user is not associated with CSH, and cannot use the Gatekeeper API.
     */
    #[default]
    GOOGLE,
}

/**
 * A game from the Devcade API
 */
#[derive(Default, Debug, Serialize, Deserialize)]
pub struct DevcadeGame {
    /**
     * The author's username, or the author's google username if the author is not a CSH member.
     */
    pub author: String,

    /**
     * The description of the game, as provided by the author.
     */
    pub description: String,

    /**
     * The hash of the game, used to verify the integrity of the game, and to determine whether the
     * game has been updated.
     */
    pub hash: String,

    /**
     * The game's ID, used to identify the game. This will not change even if the game is updated.
     */
    pub id: String,

    /**
     * The name of the game, as provided by the author.
     */
    pub name: String,

    /**
     * The tags associated with the game, used to categorize and filter games.
     */
    pub tags: Vec<Tag>,

    /**
     * The date the game was uploaded, in the format YYYY-MM-DD.
     */
    pub upload_date: String,

    /**
     * The user that uploaded the game.
     */
    pub user: User,
}

/**
 * A game from the Devcade API, but with less information. This is returned by the route that gets
 * games by tag. This is used to reduce the amount of data that needs to be sent over the network,
 * but also caused a weird bug that took me way too long to figure out.
 *
 * For per-field documentation, see the `DevcadeGame` struct.
 */
#[derive(Default, Debug, Serialize, Deserialize)]
pub struct MinimalGame {
    pub id: String,
    pub author: String,
    pub upload_date: String,
    pub name: String,
    pub hash: String,
    pub description: String,
}
