﻿#pragma checksum "..\..\..\Windows\Tambah Satuan.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "771164E8C11BF37E3AE5EDBF96197B35C89A144AAEA24AC2437932BB6EBCE864"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using MaterialDesignThemes.Wpf.Transitions;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Stokuntan.Windows {
    
    
    /// <summary>
    /// Tambah_Satuan
    /// </summary>
    public partial class Tambah_Satuan : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\..\Windows\Tambah Satuan.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnClose;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Windows\Tambah Satuan.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxNamaSatuan;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\Windows\Tambah Satuan.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxDalamGram;
        
        #line default
        #line hidden
        
        
        #line 76 "..\..\..\Windows\Tambah Satuan.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSimpanSatuan;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\Windows\Tambah Satuan.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnBatalSimpan;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Stokuntan;component/windows/tambah%20satuan.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\Tambah Satuan.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.BtnClose = ((System.Windows.Controls.Button)(target));
            
            #line 38 "..\..\..\Windows\Tambah Satuan.xaml"
            this.BtnClose.Click += new System.Windows.RoutedEventHandler(this.BtnClose_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.txtBoxNamaSatuan = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.txtBoxDalamGram = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.BtnSimpanSatuan = ((System.Windows.Controls.Button)(target));
            
            #line 78 "..\..\..\Windows\Tambah Satuan.xaml"
            this.BtnSimpanSatuan.Click += new System.Windows.RoutedEventHandler(this.BtnSimpanSatuan_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.BtnBatalSimpan = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\Windows\Tambah Satuan.xaml"
            this.BtnBatalSimpan.Click += new System.Windows.RoutedEventHandler(this.BtnBatalSimpan_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
