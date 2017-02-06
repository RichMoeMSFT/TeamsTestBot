//--------------------------------------------------------------------
// <copyright file="IAnswer.cs" company="Microsoft">
// Copyright (c) Microsoft.  All rights reserved.
// </copyright>
//--------------------------------------------------------------------

namespace TestBotCSharp
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Connector;

    /// <summary>
    /// The layout for attachments
    /// </summary>
    public enum AttachmentLayout
    {
        /// <summary>
        /// Renders attachments are carousel
        /// </summary>
        Carousel,

        /// <summary>
        /// Renders attachments as list
        /// </summary>
        List
    }

    /// <summary>
    /// Interface defining answer schema.
    /// </summary>
    public class IAnswer
    {
        /// <summary>
        /// Gets or sets list of attachments.
        /// </summary>
        IList<Attachment> Attachments
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the answer content.
        /// </summary>
        string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        string ContentFormat
        {
            get;
            set;
        }

  
        /// <summary>
        /// Gets or sets the attachment layout
        /// </summary>
        AttachmentLayout AttachmentLayout
        {
            get;
            set;
        }
    }
}