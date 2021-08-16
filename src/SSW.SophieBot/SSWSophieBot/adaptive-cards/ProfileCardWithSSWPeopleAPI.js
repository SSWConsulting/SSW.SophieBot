const cardPayload = {
    "type": "AdaptiveCard",
    "body": [
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "items": [
                        {
                            "type": "Image",
                            "style": "Person",
                            "size": "Small",
                            "width": "60px",
                            "height": "60px",
							"url": "${if(profile.avatarUrl != null, profile.avatarUrl, '')}"
                        }
                    ],
                    "width": "auto"
                },
                {
                    "type": "Column",
                    "items": [
                        {
                            "type": "TextBlock",
                            "weight": "Bolder",
                            "text": "${profile.displayName}",
                            "size": "Large",
                            "wrap": true
                        },
                        {
                            "type": "TextBlock",
                            "spacing": "None",
                            "isSubtle": true,
                            "wrap": true,
                            "text": "${profile.title}"
                        }
                    ],
                    "width": "stretch",
                    "verticalContentAlignment": "Center"
                }
            ]
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "TextBlock",
                            "wrap": true,
                            "text": "${profile.basicInfo}",
                            "isVisible": false
                        },
                        {
                            "type": "TextBlock",
                            "wrap": true,
                            "text": "${if(profile.lastSeenAt != null, profile.lastSeenInfo, '')}"
                        }
                    ]
                }
            ]
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "Skills",
                            "wrap": true,
                            "size": "Small",
                            "weight": "Bolder"
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {
                                    "type": "Column",
                                    "width": "auto",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "wrap": true,
                                            "text": "Show more",
                                            "size": "Small",
                                            "color": "Accent",
                                            "horizontalAlignment": "Right",
                                            "id": "showMore"
                                        },
                                        {
                                            "type": "TextBlock",
                                            "text": "Show less",
                                            "wrap": true,
                                            "size": "Small",
                                            "color": "Accent",
                                            "horizontalAlignment": "Right",
                                            "isVisible": false,
                                            "id": "showLess"
                                        }
                                    ],
                                    "selectAction": {
                                        "type": "Action.ToggleVisibility",
                                        "targetElements": [
                                            "showMore",
                                            "showLess",
                                            "omittedSkills",
                                            "allSkills"
                                        ]
                                    }
                                }
                            ],
                            "horizontalAlignment": "Right"
                        }
                    ]
                }
            ],
            "separator": true
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "auto",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "${profile.skillsInfo}",
                            "wrap": true,
                            "id": "omittedSkills",
                            "maxLines": 2
                        },
                        {
                            "type": "ColumnSet",
                            "columns": [
                                {
                                    "type": "Column",
                                    "width": "stretch",
                                    "items": [
                                        {
                                            "type": "TextBlock",
                                            "text": "${profile.skillsInfo}",
                                            "isVisible": false,
                                            "id": "allSkills",
                                            "height": "stretch",
                                            "wrap": true
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        },
        {
            "type": "TextBlock",
            "text": "Contact",
            "wrap": true,
            "weight": "Bolder",
            "size": "Small",
            "separator": true
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "auto",
                    "items": [
                        {
                            "type": "Image",
                            "altText": "email",
                            "width": "20px",
                            "height": "20px",
                            "url": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QA/wD/AP+gvaeTAAABm0lEQVRIie2UsWrbUBSGvyPbGAppMd3cZG+GQoc2D9CttJS+QcFYR8bEnTOKTilkKC2YXlleunrokCGPkCXpO5TGeAn2EgrGjk4XKQi7dhJsKIV8IJDuOff7OVdCcMe/RrIbVX0FOODRis4zM9NOp3ME4OUKX9cgB9gUEZc9FPMFgCiKZG7LLVBVA7b+FgBAEASH0+l0t9vt/ryNuFarVYvF4qfZ9bkAM3tdKBReBEHwYTgcHvR6vctl4jAMvcFgUDezA2Bjtu7NLojIG+DczPYrlcpJo9HYWST3ff9pv98/NjMHXIjIu2sDnHOHwLaZfQSeJElyrKqu1Wrdz3pU9Z7v+/siciIiz4CoXC4/ds59m/XNHRFAFEW/gb16vd7zPC8CdDwev1TV92nLZxHZAn4kSaJxHJ8umnJugjxxHJ9Wq9XnIhIAD4Dv6fVQRPZGo9HOMvnCCfKEYZgAUbPZPJpMJl8ASqVSq91u/7pu740CMlLh25v2Zyw9onXw/wfk38EZsJn+S1bl6gO4msDMNA1ZWS4iugbPHWviD11fh8+hcqtaAAAAAElFTkSuQmCC"
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "TextBlock",
                            "text": "${profile.emailAddress}",
                            "wrap": true
                        }
                    ],
                    "verticalContentAlignment": "Center"
                }
            ],
            "height": "stretch"
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "auto",
                    "items": [
                        {
                            "type": "Image",
                            "altText": "phone number",
                            "url": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QA/wD/AP+gvaeTAAABxklEQVRIie2Rv2tTURTHP19fnim04KAGVxGCgz/KM1MRKrpVUBBXhww3gSBuIojCQxdH13dTMvUvaOngINJBdEhCKqKTIrg5KKiYal7edbCRZ/JKappNv9M953zP9wP3wH+NkdKFMeaJpHOp1g9JV6MoWpsUsO8PmrQwNN/vnFucNHwEAHweNkg6OE3Au2GDc+7U1ACSXmZ45mu12txUAEmSbGR4OoVCYWsqgDiO14FeqvU6SZKlMAzjSQFeuuh0Ot+CIDgj6TjwptfrnW00Gh8mDYfRI+Oce7D9PJLP52f3Ep4JWF5efg6sA7P9fv/h1AEAnufdAL5KulytVm/tBeBlNZvN5qdSqfQeuAJcCILgbbvdfjHsC8MwVywWZ1qtVm805Ze00wDAGHNf0h3AAbettYP7UC6XD/u+/wg4DWxK2kyS5COwUa/XV3cFAGSMubcNAVjN5XLXu93ulu/7j4GTGTvfrbUzgyLzBim5er1+V9I14AtwKY7jV77vP9shHCCfLsYBAIiiaMXzvHlgDZgDju1mD8Z/0YiMMQuSbgIXAT/LY639nfvXgIEqlcohSUvOuUXn3AlJR4EDwFNr7flJc/9B/QT46YL539BpLgAAAABJRU5ErkJggg==",
                            "width": "20px",
                            "height": "20px"
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "TextBlock",
                            "wrap": true,
                            "text": "${profile.mobilePhone}"
                        }
                    ],
                    "verticalContentAlignment": "Center"
                }
            ]
        },
        {
            "type": "ColumnSet",
            "columns": [
                {
                    "type": "Column",
                    "width": "auto",
                    "items": [
                        {
                            "type": "Image",
                            "altText": "location",
                            "url": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAABmJLR0QA/wD/AP+gvaeTAAACaElEQVRIidWUQUgUURjHf9/Mzm5d1oi6dSnXe6SH6NAlCYIwQjAjIkX3TSJdokNEBIle65LivFGwgqAUQZDs3FWUupp2qk5G69FtZ74OzcKyzOhaJ/+3977/9/+9783w4LBL9jMYY06IyD1VvQqUAETki6ouAy+stdv/DCiXy9dFZA4oZlh2ROROEARLWRnOPuELQFFEluM4vlSr1Yq1Wq0oIt2q+h5oU9VF3/evHWiCwcHBk57nbSYnf2ytncg4xBMReQpUPM8rTU5O/mz25NIaPc8brZ88CIIJgIGBgWOFQqEPQFXfWmt3wjAcM8acB65Uq9VRYKw5K+uKegCiKHpeD8/n8+uqGqhqAKwbY9oS2DMAx3F60oKyAO0AcRyvAhQKhX7gdEP9jIjcABCR1QRUOgjgdwJwk3XcbFDVOJnSbexpFbAB4HleF8Du7u474GtDfQuYB3Bdt6uxp1XACkD9Gubm5irAOVU1qmqATmvtTuLtT7wraUGpv+nQ0FDJdd0NoOo4Tvv09PT3NN/w8PApx3G2AA/osNZutTTB7OzsJrAEFKIoup8xJa7rPgDywGJaeCYAII7jcUBFZMQY095c932/Q1V9IFbV8aycTMDMzMyaqr4GjgJTzXVVnQKOAC/DMPx0YACAiDwEKsBl3/dv1vfL5fJtoBuo5HK5R3tm7FWsh4nIK6ASRdFZEYkcx/kMHAduWWvf/BcAwBizAPQCH5Oti8C8tbZvv97Uxy5Fd4ELSTDAD8/zRlpp3PMb1GWt3RaRXqAK1OI47k97mtPk7m/5q7W1tW+dnZ2/gA9hGC602nf49Qd/ROaNppsWuQAAAABJRU5ErkJggg==",
                            "width": "20px",
                            "height": "20px"
                        }
                    ]
                },
                {
                    "type": "Column",
                    "width": "stretch",
                    "items": [
                        {
                            "type": "TextBlock",
                            "wrap": true,
                            "text": "${profile.defaultSite.name}"
                        }
                    ],
                    "verticalContentAlignment": "Center"
                }
            ]
        },
        {
            "type": "ActionSet",
            "actions": [
                {
                    "type": "Action.OpenUrl",
                    "title": "More about ${profile.firstName}",
                    "url": "https://www.ssw.com.au/people/${toLower(profile.firstName)}-${toLower(profile.lastName)}"
                }
            ]
        }
    ],
    "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
    "version": "1.2",
    "minHeight": "100px"
}

const sampleData = {
    "profile": {
        "avatarUrl": "https://secure.gravatar.com/avatar/2b330f81912f60c233c5b08fadc5b0e8?s=100&d=identicon",
        "displayName": "Luke Mao",
        "title": "Senior Software Architect",
        "basicInfo": "Is Busy in Hangzhou working for SSW.Bots today.",
        "lastSeenInfo": "Last seen in Hangzhou.",
        "SkillsInfo": "PowerShell, Blazor, GraphQL, .NET Core, Mobile Apps - Xamarin, Content writing, Adobe Premiere Pro, Hyper V, Azure, Angular, Clean Architecture, .NET, Azure AD B2C, CMS - Contentful, EF Core, Scrum and Training",
        "emailAddress": "lukemao@ssw.com.au",
        "mobilePhone": "+86 15249271572",
        "defaultSite": {
            "name": "Remote"
        },
        "firstName": "Luke",
        "LastName": "Mao",
        "lastSeenAt": {
            "siteId": "Hangzhou"
        }
    }
}