using System;

namespace onboard.devcade;

public class DevcadeGame {
    public string id { get; }
    public string author { get; }
    public DateTime uploadDate { get; }
    public string name { get; }
    public string hash { get; }
    public string description { get; }
    public string iconLink { get; }
    public string bannerLink { get; }

    public DevcadeGame(string id, string author, DateTime uploadDate, string name, string hash, string description,
        string iconLink, string bannerLink) {
        this.id = id;
        this.author = author;
        this.uploadDate = uploadDate;
        this.name = name;
        this.hash = hash;
        this.description = description;
        this.iconLink = iconLink;
        this.bannerLink = bannerLink;
    }

    public DevcadeGame(string id, string name) {
        this.id = id;
        this.author = "";
        this.uploadDate = DateTime.Now;
        this.name = name;
        this.hash = "";
        this.description = "";
        this.iconLink = "";
        this.bannerLink = "";
    }

    public DevcadeGame() {
        this.id = "";
        this.author = "";
        this.uploadDate = DateTime.Now;
        this.name = "";
        this.hash = "";
        this.description = "";
        this.iconLink = "";
        this.bannerLink = "";
    }
}
