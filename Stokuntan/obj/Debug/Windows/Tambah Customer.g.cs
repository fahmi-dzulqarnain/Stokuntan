﻿#pragma checksum "..\..\..\Windows\Tambah Customer.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "3210835284EA7071F9FAE70B17F5BDC9A381ED1E2D8550A69E071DDCC9F21FAF"
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
    /// Tambah_Customer
    /// </summary>
    public partial class Tambah_Customer : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnClose;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxNamaCustomer;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxKontak;
        
        #line default
        #line hidden
        
        
        #line 75 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxDiskon;
        
        #line default
        #line hidden
        
        
        #line 85 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtBoxAlamat;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\Windows\Tambah Customer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button BtnSimpanCustomer;
        
        #line default
        #line hidden
        
        
        #line 94 "..\..\..\Windows\Tambah Customer.xaml"
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
            System.Uri resourceLocater = new System.Uri("/Stokuntan;component/windows/tambah%20customer.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\Tambah Customer.xaml"
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
            
            #line 38 "..\..\..\Windows\Tambah Customer.xaml"
            this.BtnClose.Click += new System.Windows.RoutedEventHandler(this.BtnClose_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.txtBoxNamaCustomer = ((System.Windows.Controls.TextBox)(target));
            return;
            case 3:
            this.txtBoxKontak = ((System.Windows.Controls.TextBox)(target));
            return;
            case 4:
            this.txtBoxDiskon = ((System.Windows.Controls.TextBox)(target));
            
            #line 78 "..\..\..\Windows\Tambah Customer.xaml"
            this.txtBoxDiskon.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.TxtBoxDiskon_TextChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.txtBoxAlamat = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            this.BtnSimpanCustomer = ((System.Windows.Controls.Button)(target));
            
            #line 93 "..\..\..\Windows\Tambah Customer.xaml"
            this.BtnSimpanCustomer.Click += new System.Windows.RoutedEventHandler(this.BtnSimpanCustomer_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.BtnBatalSimpan = ((System.Windows.Controls.Button)(target));
            
            #line 97 "..\..\..\Windows\Tambah Customer.xaml"
            this.BtnBatalSimpan.Click += new System.Windows.RoutedEventHandler(this.BtnBatalSimpan_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
