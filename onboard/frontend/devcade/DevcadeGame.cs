using System;

namespace onboard.devcade; 

public class DevcadeGame
{
    public string id { get; set; }
    public string author { get; set; }
    public DateTime uploadDate { get; set; }
    public string name { get; set; }
    public string hash { get; set; }
    public string description { get; set; }
    public string iconLink { get; set; }
    public string bannerLink { get; set; }
    
    public DevcadeGame(string id, string author, DateTime uploadDate, string name, string hash, string description, string iconLink, string bannerLink)
    {
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
    
    public DevcadeGame()
    {
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